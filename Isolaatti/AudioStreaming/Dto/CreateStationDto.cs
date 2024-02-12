using System;
using Isolaatti.AudioStreaming.Entity;

namespace Isolaatti.AudioStreaming.Dto;

public record CreateStationDto(string Name, string Description);

public record CreateStationResultDto(RadioStationEntity Station);