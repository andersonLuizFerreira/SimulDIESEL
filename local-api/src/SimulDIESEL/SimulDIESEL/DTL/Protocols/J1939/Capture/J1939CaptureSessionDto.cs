using System;
using System.Collections.Generic;

namespace SimulDIESEL.DTL.Protocols.J1939.Capture
{
    public sealed class J1939CaptureSessionDto
    {
        public J1939CaptureSessionDto()
        {
            Id = Guid.NewGuid().ToString("N");
            Events = new List<J1939CapturedEventDto>();
        }

        public string Id { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? StoppedAt { get; set; }
        public bool IsActive { get; set; }
        public int TotalFrameCount { get; set; }
        public int UniqueFrameCount { get; set; }
        public List<J1939CapturedEventDto> Events { get; set; }

        public long DurationMs
        {
            get
            {
                DateTime end = StoppedAt.HasValue ? StoppedAt.Value : DateTime.Now;
                if (StartedAt == default(DateTime) || end < StartedAt)
                    return 0;

                return (long)(end - StartedAt).TotalMilliseconds;
            }
        }
    }
}
