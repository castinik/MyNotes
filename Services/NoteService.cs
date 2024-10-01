using MyNotes.Models;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Text;
using Note = MyNotes.Models.Note;

namespace MyNotes.Services
{
    public interface INoteService
    {
        Task SaveNoteAsync(Note note, String folder = "");
        Task<ObservableCollection<Note>> LoadNotesAsync(String folder = "");
        Task AddFolderAsync(String folder);
        Task<ObservableCollection<Folder>> LoadFoldersAsync();
    }

    public class NoteService : INoteService
    {
        // /storage/emulated/0/Android/data/MyNotes
        // /data/user/0/com.CKStudios.mynotes/files
        // /data/user/0/com.companyname.mynotes/files
        // /storage/emulated/0

        // appdata - path in key: pathfiles

        public async Task SaveNoteAsync(Note note, String folder = "")
        {
            String pathFiles = Data.DefaultData;
            String pathFolder = String.IsNullOrEmpty(folder) ? pathFiles : $"{pathFiles}/{folder}";
            String noteJson = JsonConvert.SerializeObject(note);
            String path = Path.Combine(pathFolder, $"{note.Title}.txt");
            await File.WriteAllTextAsync(path, noteJson);
        }

        public async Task<ObservableCollection<Note>> LoadNotesAsync(String folder = "")
        {
            ObservableCollection<Note> notes = new ObservableCollection<Note>();
            String[] files = Array.Empty<String>();

            String pathFiles = Data.DefaultData;
            String path = String.IsNullOrEmpty(folder) ? pathFiles : $"{pathFiles}/{folder}";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            
            files = Directory.GetFiles(path, "*.txt");

            foreach (String file in files)
            {
                String fileJson = File.ReadAllText(file);
                Note note = JsonConvert.DeserializeObject<Note>(fileJson);
                notes.Add(note);
            }

            return notes;
        }

        public async Task AddFolderAsync(String folderName)
        {
            String pathFiles = Data.DefaultData;
            String path = Path.Combine(pathFiles, folderName);
            Directory.CreateDirectory(path);
        }

        public async Task<ObservableCollection<Folder>> LoadFoldersAsync()
        {
            ObservableCollection<Folder> folders = new ObservableCollection<Folder>();
            String[] files = Array.Empty<String>();
            //var files = Directory.GetFiles(FileSystem.AppDataDirectory, "*.txt");
            String pathFiles = Data.DefaultData;

            if (!Directory.Exists(pathFiles))
            {
                Directory.CreateDirectory(pathFiles);
            }

            files = Directory.GetDirectories(pathFiles);

            foreach (String file in files)
            {
                Folder folder = new Folder();
                String folderName = file.Split('/').Last();
                if (!String.IsNullOrEmpty(folderName))
                {
                    folder.Name = folderName;
                }
                else
                {
                    folder.Name = file.Split('\\').Last();
                }
                folders.Add(folder);
            }
            return folders;
        }

        public static Boolean DeleteNote(Note note, String folder = "")
        {
            try
            {
                String pathFiles = Data.DefaultData;
                String pathFolder = String.IsNullOrEmpty(folder) ? pathFiles : $"{pathFiles}/{folder}";
                String path = Path.Combine(pathFolder, $"{note.Title}.txt");
                File.Delete(path);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    
        public static Boolean DeleteFolder(String folder)
        {
            try
            {
                String pathFiles = Data.DefaultData;
                String path = Path.Combine(pathFiles, folder);
                Directory.Delete(path, true);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    
        public static Boolean WriteInfo(String key, String value)
        {
            String fileName = Path.Combine(FileSystem.AppDataDirectory, "appdata.txt");
            List<String> lines = new List<String>();
            // struttura chiave valore dei dati dell'app
            List<KeyValue> appdata = new List<KeyValue>();

            if (File.Exists(fileName))
            {
                using (StreamReader streamReader = new StreamReader(fileName, Encoding.UTF8))
                {
                    while (!streamReader.EndOfStream)
                    {
                        lines.Add(streamReader.ReadLine());
                    }
                }

                foreach (String line in lines)
                {
                    try
                    {
                        KeyValue keyValue = JsonConvert.DeserializeObject<KeyValue>(line);
                        if (keyValue.Key == key)
                        {
                            keyValue.Value = value;
                        }
                        appdata.Add(keyValue);
                    }
                    catch (Exception ex)
                    {

                    }
                }

                File.Delete(fileName);
            }
            else
            {
                KeyValue keyValue = new KeyValue()
                {
                    Key = key,
                    Value = value
                };
                appdata.Add(keyValue);
            }

            foreach (KeyValue keyValue in appdata)
            {
                using (StreamWriter streamWriter = new StreamWriter(fileName))
                {
                    String content = JsonConvert.SerializeObject(keyValue);
                    streamWriter.WriteLine(content);
                }
            }

            return true;
        }
    
        public static String ReadInfo(String key)
        {
            String fileName = Path.Combine(FileSystem.AppDataDirectory, "appdata.txt");
            List<String> lines = new List<String>();

            if (File.Exists(fileName))
            {
                using (StreamReader streamReader = new StreamReader(fileName))
                {
                    while (!streamReader.EndOfStream)
                    {
                        lines.Add(streamReader.ReadLine());
                    }
                }

                foreach (String line in lines)
                {
                    KeyValue keyValue = JsonConvert.DeserializeObject<KeyValue>(line);
                    if (keyValue.Key == key)
                        return keyValue.Value;
                }
            }

            return null;
        }
    }
}
