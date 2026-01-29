using System;
using System.Collections.Generic;
using System.Text;

namespace MessageAPI.Domain.DbModels
{
    public abstract class EntityBase
    {
        public int Id { get; set; }

        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? UpdatedAtUtc { get; private set; }

        public string? CreatedBy { get; private set; }
        public string? UpdatedBy { get; private set; }

        public void SetCreated(DateTime utcNow, string? user)
        {
            CreatedAtUtc = utcNow;
            CreatedBy = user;
        }

        public void SetUpdated(DateTime utcNow, string? user)
        {
            UpdatedAtUtc = utcNow;
            UpdatedBy = user;
        }
    }
}
