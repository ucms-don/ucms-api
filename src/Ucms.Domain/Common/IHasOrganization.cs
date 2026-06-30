namespace Ucms.Domain.Common;

using System;

public interface IHasOrganization
{
    Guid OrganizationId { get; set; }
}
