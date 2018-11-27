using System;
using System.Collections.Generic;

using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Smo.Common;
using Smo.Common.Enums;

namespace SmoReader.Utils
{
    public interface IFilePersistence
    {
        T GetFromFile<T>(string path);
        void SaveToFile<T>(T serializable, string path);
    }

    public class FilePersistence : IFilePersistence
    {
        private readonly ILoggingService _loggingService;

        public FilePersistence(ILoggingService loggingService = null)
        {
            _loggingService = loggingService;
        }

        public T GetFromFile<T>(string path)
        {
            T serialzable = default(T);

            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();

                    serialzable = (T)formatter.Deserialize(fs);
                };
            }
            catch (Exception e)
            {
                _loggingService?.Log("Failed to deserialize. Reason: " + e.Message);

            }

            return serialzable;
        }

        public void SaveToFile<T>(T serializable, string path)
        {

            System.IO.Directory.CreateDirectory(Path.GetDirectoryName(path));

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    formatter.Serialize(fs, serializable);
                }
                catch (SerializationException e)
                {
                    _loggingService?.Log("Failed to serialize. Reason: " + e.Message, LogLevel.Error);
                    throw;
                }
            };
        }
    }
}
