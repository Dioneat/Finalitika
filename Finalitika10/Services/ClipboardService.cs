using System;
using System.Collections.Generic;
using System.Text;

namespace Finalitika10.Services
{
    public sealed class ClipboardService : IClipboardService
    {
        public Task SetTextAsync(string text) =>
            Clipboard.Default.SetTextAsync(text);
    }

    public interface IClipboardService
    {
        Task SetTextAsync(string text);
    }
}
