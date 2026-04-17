using CommunityToolkit.Maui.Media;
using Finalitika10.Services.Interfaces;
using System.Globalization;

namespace Finalitika10.Services
{
    public sealed class AppSpeechToTextService : IAppSpeechToTextService
    {
        public async Task<string?> ListenOnceAsync(string cultureName = "ru-RU", CancellationToken cancellationToken = default)
        {
            var granted = await SpeechToText.Default.RequestPermissions(cancellationToken);
            if (!granted)
                throw new InvalidOperationException("Нет доступа к микрофону");

            var tcs = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);

            void CompletedHandler(object? sender, SpeechToTextRecognitionResultCompletedEventArgs e)
            {
                SpeechToText.Default.RecognitionResultCompleted -= CompletedHandler;

                if (e.RecognitionResult != null &&
                    e.RecognitionResult.IsSuccessful &&
                    !string.IsNullOrWhiteSpace(e.RecognitionResult.Text))
                {
                    tcs.TrySetResult(e.RecognitionResult.Text);
                }
                else
                {
                    tcs.TrySetResult(null);
                }
            }

            using var ctr = cancellationToken.Register(() =>
            {
                SpeechToText.Default.RecognitionResultCompleted -= CompletedHandler;
                tcs.TrySetCanceled(cancellationToken);
            });

            SpeechToText.Default.RecognitionResultCompleted -= CompletedHandler;
            SpeechToText.Default.RecognitionResultCompleted += CompletedHandler;

            var options = new SpeechToTextOptions
            {
                Culture = CultureInfo.GetCultureInfo(cultureName),
                ShouldReportPartialResults = false
            };

            await SpeechToText.Default.StartListenAsync(options, cancellationToken);

            return await tcs.Task;
        }

        public async Task StopListeningAsync(CancellationToken cancellationToken = default)
        {
            await SpeechToText.Default.StopListenAsync(cancellationToken);
        }
    }
}