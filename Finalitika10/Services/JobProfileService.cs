using Finalitika10.Models;
using System.Text.Json;

namespace Finalitika10.Services
{
    public interface IJobProfileService
    {
        JobProfile GetProfile();
        void SaveProfile(JobProfile profile);
    }

    public class JobProfileService : IJobProfileService
    {
        private const string JobProfileKey = "UserJobProfile";

        public JobProfile GetProfile()
        {
            var json = Preferences.Default.Get(JobProfileKey, string.Empty);
            if (string.IsNullOrEmpty(json))
            {
                return new JobProfile();
            }

            return JsonSerializer.Deserialize<JobProfile>(json) ?? new JobProfile();
        }

        public void SaveProfile(JobProfile profile)
        {
            var json = JsonSerializer.Serialize(profile);
            Preferences.Default.Set(JobProfileKey, json);
        }
    }
}