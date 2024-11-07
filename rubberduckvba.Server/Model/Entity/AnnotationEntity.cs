﻿namespace rubberduckvba.Server.Model.Entity;

public record class AnnotationEntity : Entity
{
    public int FeatureId { get; init; }
    public int TagAssetId { get; init; }
    public string SourceUrl { get; init; } = string.Empty;
    public string? JsonParameters { get; init; }
    public string? JsonExamples { get; init; }

}