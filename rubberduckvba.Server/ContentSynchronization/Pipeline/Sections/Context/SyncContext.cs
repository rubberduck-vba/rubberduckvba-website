﻿using System.Collections.Immutable;
using rubberduckvba.com.Server.Data;

namespace rubberduckvba.com.Server.ContentSynchronization.Pipeline;

public interface IPipelineContext
{
    IRequestParameters Parameters { get; }
}

public class SyncContext : IPipelineContext
{
    public SyncContext(IRequestParameters parameters)
    {
        _parameters = parameters;
    }

    private IRequestParameters? _parameters;
    public IRequestParameters Parameters => ContextNotInitializedException.ThrowIfNull(_parameters);
    IRequestParameters IPipelineContext.Parameters => Parameters;    
    public void LoadParameters(SyncRequestParameters parameters)
    {
        InvalidContextParameterException.ThrowIfNull(nameof(parameters), parameters);
        //ContextAlreadyInitializedException.ThrowIfNotNull(_parameters);
        _parameters = parameters;
        _staging = new StagingContext(parameters);
    }

    private StagingContext? _staging;
    public StagingContext StagingContext => ContextNotInitializedException.ThrowIfNull(_staging);


    private TagGraph? _main;
    public TagGraph RubberduckDbMain 
        => ContextNotInitializedException.ThrowIfNull(_main);

    public void LoadRubberduckDbMain(TagGraph main)
    {
        InvalidContextParameterException.ThrowIfNull(nameof(main), main);
        ContextAlreadyInitializedException.ThrowIfNotNull(_main);
        _main ??= main;
    }

    private TagGraph? _next;
    public TagGraph RubberduckDbNext 
        => ContextNotInitializedException.ThrowIfNull(_next);

    public void LoadRubberduckDbNext(TagGraph next)
    {
        InvalidContextParameterException.ThrowIfNull(nameof(next), next);
        ContextAlreadyInitializedException.ThrowIfNotNull(_next);
        _next ??= next;
    }

    private ImmutableDictionary<int, Tag>? _tagsById;
    public bool TryGetTagById(int tagId, out Tag tag)
    {
        var tags = ContextNotInitializedException.ThrowIfNull(_tagsById);
        return tags.TryGetValue(tagId, out tag!);
    }

    public void LoadDbTags(IEnumerable<Tag> tags)
    {
        InvalidContextParameterException.ThrowIfNullOrEmpty(nameof(tags), tags);
        ContextAlreadyInitializedException.ThrowIfNotNull(_tagsById);
        _tagsById = tags.ToImmutableDictionary(tag => tag.Id);
    }

    private ImmutableDictionary<string, InspectionDefaultConfig>? _inspectionConfig;
    public ImmutableDictionary<string, InspectionDefaultConfig> InspectionDefaultConfig 
        => ContextNotInitializedException.ThrowIfNull(_inspectionConfig);
    public void LoadInspectionDefaultConfig(IEnumerable<InspectionDefaultConfig> config)
    {
        InvalidContextParameterException.ThrowIfNullOrEmpty(nameof(config), config);
        ContextAlreadyInitializedException.ThrowIfNotNull(_inspectionConfig);
        _inspectionConfig = config.ToImmutableDictionary(e => e.InspectionName);
    }

    private ImmutableHashSet<FeatureXmlDoc>? _featureItems;
    public ImmutableHashSet<FeatureXmlDoc> FeatureItems
        => ContextNotInitializedException.ThrowIfNull(_featureItems);

    public void LoadFeatureItems(IEnumerable<FeatureXmlDoc> items)
    {
        ContextAlreadyInitializedException.ThrowIfNotNull(_featureItems);
        _featureItems = items.ToImmutableHashSet();
    }

    private TagGraph? _githubMain;
    private TagGraph? _githubNext;
    private ImmutableHashSet<TagGraph>? _githubOthers;

    public TagGraph GitHubMain 
        => ContextNotInitializedException.ThrowIfNull(_githubMain);
    public TagGraph GitHubNext
        => ContextNotInitializedException.ThrowIfNull(_githubNext);
    public ImmutableHashSet<TagGraph> GitHubOtherTags
        => ContextNotInitializedException.ThrowIfNull(_githubOthers);

    public void LoadGitHubTags(TagGraph main, TagGraph next, IEnumerable<TagGraph>? others = default)
    {
        others ??= [];
        InvalidContextParameterException.ThrowIfNull(nameof(main), main);
        InvalidContextParameterException.ThrowIfNull(nameof(next), next);
        ContextAlreadyInitializedException.ThrowIfNotNull(_githubMain);
        ContextAlreadyInitializedException.ThrowIfNotNull(_githubNext);
        ContextAlreadyInitializedException.ThrowIfNotNull(_githubOthers);

        _githubMain = main;
        _githubNext = next;
        _githubOthers = others.ToImmutableHashSet();
    }

    private ImmutableDictionary<string, FeatureGraph>? _features;
    public ImmutableDictionary<string, FeatureGraph> Features
        => ContextNotInitializedException.ThrowIfNull(_features);

    public void LoadFeatures(IEnumerable<FeatureGraph> features)
    {
        InvalidContextParameterException.ThrowIfNullOrEmpty(nameof(features), features);
        ContextAlreadyInitializedException.ThrowIfNotNull(_features);
        _features = features.ToImmutableDictionary(e => e.Name);
    }
}

public class InvalidContextParameterException : ArgumentOutOfRangeException
{
    public static void ThrowIfNull<T>(string paramName, T? value)
    {
        _ = value ?? throw new InvalidContextParameterException(paramName, "Reference cannot be null");
    }

    public static void ThrowIfNullOrEmpty<T>(string paramName, IEnumerable<T> values)
    {
        if (values is null)
        {
            throw new InvalidContextParameterException(paramName, "Reference cannot be null");
        }
        if (!values.Any(e => e != null))
        {
            throw new InvalidContextParameterException(paramName, "Enumerable cannot be empty");
        }
    }

    private InvalidContextParameterException(string paramName, string message) 
        : base(paramName, message) 
    { }
}

public class ContextNotInitializedException : InvalidOperationException 
{
    public static T ThrowIfNull<T>(T? value)
        => value ?? throw new ContextNotInitializedException();
}

public class ContextAlreadyInitializedException : InvalidOperationException
{
    public static void ThrowIfNotNull<T>(params T?[] values)
    {
        if (values.Any(e => e != null))
        {
            throw new ContextAlreadyInitializedException();
        }
    }
}