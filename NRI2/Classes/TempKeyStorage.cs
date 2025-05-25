using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    public static class TempKeyStorage
    {
        private const string FileName = "temp_secret_key.txt";

        public static string LoadKey()
        {
            try
            {
                using var isoStore = IsolatedStorageFile.GetUserStoreForAssembly();
                if (isoStore.FileExists(FileName))
                {
                    using var stream = new IsolatedStorageFileStream(FileName, FileMode.Open, isoStore);
                    using var reader = new StreamReader(stream);
                    var key = reader.ReadToEnd();
                    Debug.WriteLine($"Загружен ключ из хранилища: {key}");
                    return key;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка загрузки ключа: {ex.Message}");
            }
            return null;
        }

        public static void SaveKey(string key)
        {
            try
            {
                Debug.WriteLine($"Сохранение ключа: {key}");
                using var isoStore = IsolatedStorageFile.GetUserStoreForAssembly();
                using var stream = new IsolatedStorageFileStream(FileName, FileMode.Create, isoStore);
                using var writer = new StreamWriter(stream);
                writer.Write(key);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка сохранения ключа: {ex.Message}");
            }
        }

        public static void ClearKey()
        {
            try
            {
                Debug.WriteLine("Очистка ключа");
                using var isoStore = IsolatedStorageFile.GetUserStoreForAssembly();
                if (isoStore.FileExists(FileName))
                {
                    isoStore.DeleteFile(FileName);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка удаления ключа: {ex.Message}");
            }
        }
    }


