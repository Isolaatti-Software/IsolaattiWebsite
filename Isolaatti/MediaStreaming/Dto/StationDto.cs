using System;
using System.Collections.Generic;
using Isolaatti.MediaStreaming.Entity;

namespace Isolaatti.MediaStreaming.Dto;

public class StationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Dictionary<string, string> Urls { get; set; }
    public string ThumbnailUrl { get; set; }
    public string? Description { get; set; }

    public static StationDto FromEntity(StreamingStationEntity entity, Dictionary<string, string> urls, string thumbnailUrl)
    {
        return new StationDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Urls = urls,
            ThumbnailUrl = thumbnailUrl
        };
    }
}