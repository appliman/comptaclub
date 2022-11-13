using ComptaClub.Configuration;

namespace ComptaClub.Services
{
    public class TableStorageService 
    {
        private readonly ComptaClubSettings _settings;

        public TableStorageService(Configuration.ComptaClubSettings settings)
        {
            _settings = settings;
        }
    }
}
