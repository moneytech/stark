﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StarkPlatform.Compiler.Diagnostics.Telemetry;
using StarkPlatform.Compiler.ErrorReporting;
using StarkPlatform.Compiler.Execution;
using StarkPlatform.Compiler.Options;
using StarkPlatform.Compiler.Remote;
using StarkPlatform.Compiler.Shared.TestHooks;
using StarkPlatform.Compiler.Workspaces.Diagnostics;
using Roslyn.Utilities;

namespace StarkPlatform.Compiler.Diagnostics.EngineV2
{
    internal partial class DiagnosticIncrementalAnalyzer
    {
        // internal for testing
        internal class InProcOrRemoteHostAnalyzerRunner
        {
            private readonly DiagnosticAnalyzerService _owner;
            private readonly AbstractHostDiagnosticUpdateSource _hostDiagnosticUpdateSourceOpt;

            // TODO: this should be removed once we move options down to compiler layer
            private readonly ConcurrentDictionary<string, ValueTuple<OptionSet, CustomAsset>> _lastOptionSetPerLanguage;

            public InProcOrRemoteHostAnalyzerRunner(DiagnosticAnalyzerService owner, AbstractHostDiagnosticUpdateSource hostDiagnosticUpdateSource)
            {
                _owner = owner;
                _hostDiagnosticUpdateSourceOpt = hostDiagnosticUpdateSource;

                // currently option is a bit wierd since it is not part of snapshot and 
                // we can't load all options without loading all language specific dlls.
                // we have tracking issue for this.
                // https://github.com/dotnet/roslyn/issues/13643
                _lastOptionSetPerLanguage = new ConcurrentDictionary<string, ValueTuple<OptionSet, CustomAsset>>();
            }

            public async Task<DiagnosticAnalysisResultMap<DiagnosticAnalyzer, DiagnosticAnalysisResult>> AnalyzeAsync(CompilationWithAnalyzers analyzerDriver, Project project, bool forcedAnalysis, CancellationToken cancellationToken)
            {
                var workspace = project.Solution.Workspace;
                if (!workspace.Options.GetOption(RemoteFeatureOptions.DiagnosticsEnabled))
                {
                    // diagnostic service running on remote host is disabled. just run things in in proc
                    return await AnalyzeInProcAsync(analyzerDriver, project, cancellationToken).ConfigureAwait(false);
                }

                var service = project.Solution.Workspace.Services.GetService<IRemoteHostClientService>();
                if (service == null)
                {
                    // host doesn't support RemoteHostService such as under unit test
                    return await AnalyzeInProcAsync(analyzerDriver, project, cancellationToken).ConfigureAwait(false);
                }

                var remoteHostClient = await service.TryGetRemoteHostClientAsync(cancellationToken).ConfigureAwait(false);
                if (remoteHostClient == null)
                {
                    // remote host is not running. this can happen if remote host is disabled.
                    return await AnalyzeInProcAsync(analyzerDriver, project, cancellationToken).ConfigureAwait(false);
                }

                // due to OpenFileOnly analyzer, we need to run inproc as well for such analyzers
                var inProcResultTask = AnalyzeInProcAsync(CreateAnalyzerDriver(analyzerDriver, a => a.IsOpenFileOnly(project.Solution.Workspace)), project, remoteHostClient, cancellationToken);
                var outOfProcResultTask = AnalyzeOutOfProcAsync(remoteHostClient, analyzerDriver, project, forcedAnalysis, cancellationToken);

                // run them concurrently in vs and remote host
                await Task.WhenAll(inProcResultTask, outOfProcResultTask).ConfigureAwait(false);

                // make sure things are not cancelled
                cancellationToken.ThrowIfCancellationRequested();

                // merge 2 results
                return DiagnosticAnalysisResultMap.Create(
                    inProcResultTask.Result.AnalysisResult.AddRange(outOfProcResultTask.Result.AnalysisResult),
                    inProcResultTask.Result.TelemetryInfo.AddRange(outOfProcResultTask.Result.TelemetryInfo));
            }

            private Task<DiagnosticAnalysisResultMap<DiagnosticAnalyzer, DiagnosticAnalysisResult>> AnalyzeInProcAsync(
                CompilationWithAnalyzers analyzerDriver, Project project, CancellationToken cancellationToken)
            {
                return AnalyzeInProcAsync(analyzerDriver, project, client: null, cancellationToken);
            }

            private async Task<DiagnosticAnalysisResultMap<DiagnosticAnalyzer, DiagnosticAnalysisResult>> AnalyzeInProcAsync(
                CompilationWithAnalyzers analyzerDriver, Project project, RemoteHostClient client, CancellationToken cancellationToken)
            {
                if (analyzerDriver == null ||
                    analyzerDriver.Analyzers.Length == 0)
                {
                    // quick bail out
                    return DiagnosticAnalysisResultMap.Create(ImmutableDictionary<DiagnosticAnalyzer, DiagnosticAnalysisResult>.Empty, ImmutableDictionary<DiagnosticAnalyzer, AnalyzerTelemetryInfo>.Empty);
                }

                var version = await DiagnosticIncrementalAnalyzer.GetDiagnosticVersionAsync(project, cancellationToken).ConfigureAwait(false);

                // PERF: Run all analyzers at once using the new GetAnalysisResultAsync API.
                var analysisResult = await analyzerDriver.GetAnalysisResultAsync(cancellationToken).ConfigureAwait(false);

                // if remote host is there, report performance data
                var asyncToken = _owner?.Listener.BeginAsyncOperation(nameof(AnalyzeInProcAsync));
                var _ = FireAndForgetReportAnalyzerPerformanceAsync(project, client, analysisResult, cancellationToken).CompletesAsyncOperation(asyncToken);

                // get compiler result builder map
                var builderMap = analysisResult.ToResultBuilderMap(project, version, analyzerDriver.Compilation, analyzerDriver.Analyzers, cancellationToken);

                return DiagnosticAnalysisResultMap.Create(builderMap.ToImmutableDictionary(kv => kv.Key, kv => DiagnosticAnalysisResult.CreateFromBuilder(kv.Value)), analysisResult.AnalyzerTelemetryInfo);
            }

            private async Task FireAndForgetReportAnalyzerPerformanceAsync(Project project, RemoteHostClient client, AnalysisResult analysisResult, CancellationToken cancellationToken)
            {
                if (client == null)
                {
                    return;
                }

                try
                {
                    await client.TryRunCodeAnalysisRemoteAsync(
                        nameof(IRemoteDiagnosticAnalyzerService.ReportAnalyzerPerformance),
                        new object[]
                        {
                            analysisResult.AnalyzerTelemetryInfo.ToAnalyzerPerformanceInfo(_owner),
                            // +1 for project itself
                            project.DocumentIds.Count + 1
                        },
                        cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (FatalError.ReportWithoutCrashUnlessCanceled(ex))
                {
                    // ignore all, this is fire and forget method
                }
            }

            private async Task<DiagnosticAnalysisResultMap<DiagnosticAnalyzer, DiagnosticAnalysisResult>> AnalyzeOutOfProcAsync(
                RemoteHostClient client, CompilationWithAnalyzers analyzerDriver, Project project, bool forcedAnalysis, CancellationToken cancellationToken)
            {
                var solution = project.Solution;
                var snapshotService = solution.Workspace.Services.GetService<IRemotableDataService>();

                using (var pooledObject = SharedPools.Default<Dictionary<string, DiagnosticAnalyzer>>().GetPooledObject())
                {
                    var analyzerMap = pooledObject.Object;

                    analyzerMap.AppendAnalyzerMap(analyzerDriver.Analyzers.Where(a => !a.IsOpenFileOnly(project.Solution.Workspace)));
                    if (analyzerMap.Count == 0)
                    {
                        return DiagnosticAnalysisResultMap.Create(ImmutableDictionary<DiagnosticAnalyzer, DiagnosticAnalysisResult>.Empty, ImmutableDictionary<DiagnosticAnalyzer, AnalyzerTelemetryInfo>.Empty);
                    }

                    var optionAsset = GetOptionsAsset(solution, project.Language, cancellationToken);

                    var argument = new DiagnosticArguments(
                        forcedAnalysis, analyzerDriver.AnalysisOptions.ReportSuppressedDiagnostics, analyzerDriver.AnalysisOptions.LogAnalyzerExecutionTime,
                        project.Id, optionAsset.Checksum, analyzerMap.Keys.ToArray());

                    using (var session = await client.TryCreateCodeAnalysisSessionAsync(solution, cancellationToken).ConfigureAwait(false))
                    {
                        if (session == null)
                        {
                            // session is not available
                            return DiagnosticAnalysisResultMap.Create(ImmutableDictionary<DiagnosticAnalyzer, DiagnosticAnalysisResult>.Empty, ImmutableDictionary<DiagnosticAnalyzer, AnalyzerTelemetryInfo>.Empty);
                        }

                        session.AddAdditionalAssets(optionAsset);

                        var result = await session.InvokeAsync(
                            nameof(IRemoteDiagnosticAnalyzerService.CalculateDiagnosticsAsync),
                            new object[] { argument },
                            (s, c) => GetCompilerAnalysisResultAsync(s, analyzerMap, project, c), cancellationToken).ConfigureAwait(false);

                        ReportAnalyzerExceptions(project, result.Exceptions);

                        return result;
                    }
                }
            }

            private CompilationWithAnalyzers CreateAnalyzerDriver(CompilationWithAnalyzers analyzerDriver, Func<DiagnosticAnalyzer, bool> predicate)
            {
                var analyzers = analyzerDriver.Analyzers.Where(predicate).ToImmutableArray();
                if (analyzers.Length == 0)
                {
                    // return null since we can't create CompilationWithAnalyzers with 0 analyzers
                    return null;
                }

                return analyzerDriver.Compilation.WithAnalyzers(analyzers, analyzerDriver.AnalysisOptions);
            }

            private CustomAsset GetOptionsAsset(Solution solution, string language, CancellationToken cancellationToken)
            {
                // TODO: we need better way to deal with options. optionSet itself is green node but
                //       it is not part of snapshot and can't save option to solution since we can't use language
                //       specific option without loading related language specific dlls
                var options = solution.Options;

                // we have cached options
                if (_lastOptionSetPerLanguage.TryGetValue(language, out var value) && value.Item1 == options)
                {
                    return value.Item2;
                }

                // otherwise, we need to build one.
                var assetBuilder = new CustomAssetBuilder(solution);
                var asset = assetBuilder.Build(options, language, cancellationToken);

                _lastOptionSetPerLanguage[language] = ValueTuple.Create(options, asset);
                return asset;
            }

            private async Task<DiagnosticAnalysisResultMap<DiagnosticAnalyzer, DiagnosticAnalysisResult>> GetCompilerAnalysisResultAsync(Stream stream, Dictionary<string, DiagnosticAnalyzer> analyzerMap, Project project, CancellationToken cancellationToken)
            {
                // handling of cancellation and exception
                var version = await DiagnosticIncrementalAnalyzer.GetDiagnosticVersionAsync(project, cancellationToken).ConfigureAwait(false);

                using (var reader = ObjectReader.TryGetReader(stream))
                {
                    Debug.Assert(reader != null,
    @"We only ge a reader for data transmitted between live processes.
This data should always be correct as we're never persisting the data between sessions.");
                    return DiagnosticResultSerializer.Deserialize(reader, analyzerMap, project, version, cancellationToken);
                }
            }

            private void ReportAnalyzerExceptions(Project project, ImmutableDictionary<DiagnosticAnalyzer, ImmutableArray<DiagnosticData>> exceptions)
            {
                foreach (var kv in exceptions)
                {
                    var analyzer = kv.Key;
                    foreach (var diagnostic in kv.Value)
                    {
                        _hostDiagnosticUpdateSourceOpt?.ReportAnalyzerDiagnostic(analyzer, diagnostic, project);
                    }
                }
            }
        }
    }
}
