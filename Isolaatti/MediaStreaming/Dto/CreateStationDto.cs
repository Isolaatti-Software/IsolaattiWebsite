using System;
using Isolaatti.MediaStreaming.Entity;

namespace Isolaatti.MediaStreaming.Dto;

public record CreateStationDto(string Name, string Description);

public record CreateStationResultDto(StreamingStationEntity Station);