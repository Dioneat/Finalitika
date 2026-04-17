using System;
using System.Collections.Generic;
using System.Text;

namespace Finalitika10.Services.Ai
{
    public interface IAiChatContextBuilder
    {
        Task<string> BuildAsync(CancellationToken cancellationToken = default);
    }
}
