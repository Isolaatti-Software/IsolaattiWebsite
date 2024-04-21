using System;
using System.Collections.Generic;

namespace Isolaatti.Accounts.Data;

public record LogoutSessionsDto(List<Guid> Ids);