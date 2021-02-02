using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data;
using System.IO;
using RockSnifferLib.Logging;
using RockSnifferLib.Sniffing;
using Newtonsoft.Json;
using System.Drawing.Imaging;
using RockSnifferGui.Model;
using Newtonsoft.Json.Linq;
using System.Drawing;
using RockSnifferLib.RSHelpers.NoteData;
using System.Windows;
using RockSnifferGui.Common;

namespace RockSnifferGui.DataStore
{
    public class SQLiteStore
    {
        private SQLiteConnection Connection { get; set; }

        public SQLiteStore()
        {
            if (!File.Exists(SQLiteSchemaStrings.DB_FILE_NAME))
            {
                SQLiteConnection.CreateFile(SQLiteSchemaStrings.DB_FILE_NAME);
            }

            Connection = new SQLiteConnection($"Data Source={SQLiteSchemaStrings.DB_FILE_NAME};");
            Connection.Open();

            CreateTables();
        }

        private void CreateTables()
        {
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = SQLiteSchemaStrings.SongHistoryTableCreate;
                cmd.ExecuteNonQuery();
            }

            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = SQLiteSchemaStrings.SongHistoryTableIdIndex;
                cmd.ExecuteNonQuery();
            }

            // Enable WAL mode, it is MUCH faster
            // It shouldn't have any downsides in this case
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = SQLiteSchemaStrings.WriteAheadLogPragma;
                cmd.ExecuteNonQuery();
            }

            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = SQLiteSchemaStrings.SynchronousNormalPragma;
                cmd.ExecuteNonQuery();
            }

            if (Logger.logCache)
            {
                Logger.Log("SQLite database initialised");
            }
        }

        public long Add(SongPlayInstance songPlayInstance)
        {
            try
            {
                long toReturn = 0;

                SQLiteTransaction transaction = Connection.BeginTransaction();

                using (var cmd = Connection.CreateCommand())
                {
                    cmd.CommandText = SQLiteSchemaStrings.PlayHistoryInsert;

                    #region add parameter objects
                    cmd.Parameters.Add("@songid", DbType.String);
                    cmd.Parameters.Add("@songname", DbType.String);
                    cmd.Parameters.Add("@artistname", DbType.String);
                    cmd.Parameters.Add("@albumname", DbType.String);
                    cmd.Parameters.Add("@songLength", DbType.Single);
                    cmd.Parameters.Add("@albumYear", DbType.Int32);
                    cmd.Parameters.Add("@arrangements", DbType.String);
                    cmd.Parameters.Add("@album_art", DbType.Binary);
                    cmd.Parameters.Add("@notes_hit", DbType.Int32);
                    cmd.Parameters.Add("@notes_missed", DbType.Int32);
                    cmd.Parameters.Add("@total_notes", DbType.Int32);
                    cmd.Parameters.Add("@max_streak", DbType.Int32);
                    cmd.Parameters.Add("@start_time", DbType.DateTime);
                    cmd.Parameters.Add("@end_time", DbType.DateTime);
                    #endregion

                    #region populate parameter values
                    cmd.Parameters["@songid"].Value = songPlayInstance.SongDetails.songID;
                    cmd.Parameters["@songname"].Value = songPlayInstance.SongDetails.songName;
                    cmd.Parameters["@artistname"].Value = songPlayInstance.SongDetails.artistName;
                    cmd.Parameters["@albumname"].Value = songPlayInstance.SongDetails.albumName;
                    cmd.Parameters["@songLength"].Value = songPlayInstance.SongDetails.songLength;
                    cmd.Parameters["@albumYear"].Value = songPlayInstance.SongDetails.albumYear;
                    cmd.Parameters["@arrangements"].Value = JsonConvert.SerializeObject(songPlayInstance.SongDetails.arrangements);
                    // album art handled below
                    cmd.Parameters["@notes_hit"].Value = songPlayInstance.NoteData.TotalNotesHit;
                    cmd.Parameters["@notes_missed"].Value = songPlayInstance.NoteData.TotalNotesMissed;
                    cmd.Parameters["@total_notes"].Value = songPlayInstance.NoteData.TotalNotes;
                    cmd.Parameters["@max_streak"].Value = songPlayInstance.NoteData.HighestHitStreak;
                    cmd.Parameters["@start_time"].Value = songPlayInstance.EndTime;
                    cmd.Parameters["@end_time"].Value = songPlayInstance.StartTime;

                    #region handle album art
                    //cmd.Parameters["@album_art"].Value = SerializeImage(songPlayInstance.SongDetails.albumArt, songPlayInstance.imageLock);
                    #endregion

                    #endregion

                    cmd.ExecuteNonQuery();
                    toReturn = Connection.LastInsertRowId;

                    transaction.Commit();

                    if (Logger.logCache)
                    {
                        Logger.Log($"Song played {songPlayInstance.SongDetails.songName}/{songPlayInstance.SongDetails.songID}");
                    }

                    return toReturn;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                Utilities.ShowExceptionMessageBox(ex);

                throw ex;
            }
        }

        public static byte[] SerializeImage(Image image, object imageLock)
        {
            byte[] toReturn = null;

            if (image != null)
            {
                using (var ms = new MemoryStream())
                {
                    lock (imageLock)
                    {
                        image.Save(ms, ImageFormat.Png);
                        toReturn = ms.ToArray();
                    }
                }
            }

            return toReturn;
        }

        private Image DeserializeImage(byte[] imageBytes)
        {
            Image toReturn = null;

            using (var ms = new MemoryStream(imageBytes))
            {
                toReturn = Image.FromStream(ms);
            }

            return toReturn;
        }

        public SongPlayInstance Get(string SongID)
        {
            try
            {
                SongPlayInstance toReturn = null;

                using (var cmd = Connection.CreateCommand())
                {
                    cmd.CommandText = SQLiteSchemaStrings.PlayHistorySelectBySongId;
                    cmd.Parameters.AddWithValue("@songid", SongID);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var songDetails = new SongDetails
                            {
                                songID = ReadField<string>(reader, "songid"),
                                songName = ReadField<string>(reader, "songname"),
                                artistName = ReadField<string>(reader, "artistname"),
                                albumName = ReadField<string>(reader, "albumname"),
                                songLength = (float)ReadField<double>(reader, "songLength"),
                                albumYear = (int)ReadField<long>(reader, "albumYear"),
                                arrangements = JsonConvert.DeserializeObject<List<ArrangementDetails>>(ReadField<string>(reader, "arrangements")),
                                albumArt = null
                            };

                            //try
                            //{
                            //    byte[] blob = ReadField<byte[]>(reader, "album_art");
                            //    songDetails.albumArt = this.DeserializeImage(blob);
                            //}
                            //catch
                            //{

                            //}

                            var noteDetails = new LearnASongNoteData();

                            toReturn = new SongPlayInstance(songDetails, noteDetails);
                            toReturn.NotesHit = (int)ReadField<long>(reader, "notes_hit");
                            toReturn.NotesMissed = (int)ReadField<long>(reader, "notes_missed");
                            toReturn.TotalNotes = (int)ReadField<long>(reader, "total_notes");
                            toReturn.HighestHitStreak = (int)ReadField<long>(reader, "max_streak");

                        }
                    }
                }

                return toReturn;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                Utilities.ShowExceptionMessageBox(ex);

                throw ex;
            }
        }

        public List<SongPlayInstance> GetAll()
        {
            try
            {
                List<SongPlayInstance> toReturn = new List<SongPlayInstance>();

                using (var cmd = Connection.CreateCommand())
                {
                    cmd.CommandText = SQLiteSchemaStrings.PlayHistorySelectAll;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var songDetails = new SongDetails
                            {
                                songID = ReadField<string>(reader, "songid"),
                                songName = ReadField<string>(reader, "songname"),
                                artistName = ReadField<string>(reader, "artistname"),
                                albumName = ReadField<string>(reader, "albumname"),
                                songLength = (float)ReadField<double>(reader, "songLength"),
                                albumYear = (int)ReadField<long>(reader, "albumYear"),
                                //arrangements = JsonConvert.DeserializeObject<List<ArrangementDetails>>(ReadField<string>(reader, "arrangements")),
                                albumArt = null
                            };

                            //try
                            //{
                            //    byte[] blob = ReadField<byte[]>(reader, "album_art");
                            //    songDetails.albumArt = this.DeserializeImage(blob);
                            //}
                            //catch(Exception ex)
                            //{
                            //    Logger.LogException(ex);
                            //}
                            
                            var noteDetails = new LearnASongNoteData();

                            SongPlayInstance toAdd = new SongPlayInstance(songDetails, noteDetails, ReadField<DateTime>(reader, "start_time"), ReadField<DateTime>(reader, "end_time"));
                            toAdd.NotesHit = (int)ReadField<long>(reader, "notes_hit");
                            toAdd.NotesMissed = (int)ReadField<long>(reader, "notes_missed");
                            toAdd.TotalNotes = (int)ReadField<long>(reader, "total_notes");
                            toAdd.HighestHitStreak = (int)ReadField<long>(reader, "max_streak");

                            toReturn.Add(toAdd);
                        }
                    }
                }

                return toReturn;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                Utilities.ShowExceptionMessageBox(ex);

                throw ex;
            }
        }

        private T ReadField<T>(SQLiteDataReader reader, string field)
        {
            int ordinal = reader.GetOrdinal(field);

            if (reader.IsDBNull(ordinal))
            {
                return default(T);
            }

            return (T)reader.GetValue(ordinal);
        }

        public long Test()
        {
            long toReturn = 0;

            SQLiteTransaction transaction = Connection.BeginTransaction();

            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = SQLiteSchemaStrings.PlayHistoryInsert;

                cmd.Parameters.Add("@songid", DbType.String);
                cmd.Parameters.Add("@songname", DbType.String);
                cmd.Parameters.Add("@artistname", DbType.String);
                cmd.Parameters.Add("@albumname", DbType.String);
                cmd.Parameters.Add("@songLength", DbType.Single);
                cmd.Parameters.Add("@albumYear", DbType.Int32);
                cmd.Parameters.Add("@arrangements", DbType.String);
                cmd.Parameters.Add("@album_art", DbType.Binary);
                cmd.Parameters.Add("@notes_hit", DbType.Int32);
                cmd.Parameters.Add("@notes_missed", DbType.Int32);
                cmd.Parameters.Add("@total_notes", DbType.Int32);
                cmd.Parameters.Add("@max_streak", DbType.Int32);
                cmd.Parameters.Add("@start_time", DbType.DateTime);
                cmd.Parameters.Add("@end_time", DbType.DateTime);

                cmd.Parameters["@songid"].Value = 1234;
                cmd.Parameters["@songname"].Value = "Test song name";
                cmd.Parameters["@artistname"].Value = "Test artist name";
                cmd.Parameters["@albumname"].Value = "Test album name";
                cmd.Parameters["@songLength"].Value = 180f;
                cmd.Parameters["@albumYear"].Value = 2020;
                cmd.Parameters["@arrangements"].Value = "{}";

                cmd.Parameters["@notes_hit"].Value = 10;
                cmd.Parameters["@notes_missed"].Value = 5;
                cmd.Parameters["@total_notes"].Value = 15;
                cmd.Parameters["@max_streak"].Value = 8;
                cmd.Parameters["@start_time"].Value = DateTime.UtcNow;
                cmd.Parameters["@end_time"].Value = DateTime.UtcNow.Subtract(new TimeSpan(0, 0, 180));

                cmd.Parameters["@album_art"].Value = null;

                cmd.ExecuteNonQuery();
                toReturn = Connection.LastInsertRowId;

                transaction.Commit();
            }

            return toReturn;
        }
    }

    class SQLiteSchemaStrings
    {
        public const string DB_FILE_NAME = "userdata.sqlite";
        public const string PLAYED_SONGS_TABLE_NAME = "playInstances";

        public static string SongHistoryTableCreate = $@"
            CREATE TABLE IF NOT EXISTS `{SQLiteSchemaStrings.PLAYED_SONGS_TABLE_NAME}` (
	            `id`                INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	            `songid`            TEXT NOT NULL,
	            `songname`          TEXT NOT NULL,
	            `artistname`	    TEXT NOT NULL,
	            `albumname`         TEXT,
	            `songLength`	    REAL,
	            `albumYear`         INTEGER,
	            `arrangements`	    TEXT,
                `album_art`	        BLOB,
                `notes_hit`         INTEGER,
                `notes_missed`      INTEGER,
                `total_notes`       INTEGER,
                `max_streak`        INTEGER,
                `start_time`        DATETIME,
                `end_time`          DATETIME
            );";

        public static string SongHistoryTableIdIndex = $"CREATE INDEX IF NOT EXISTS`songid` ON `{SQLiteSchemaStrings.PLAYED_SONGS_TABLE_NAME}` (`songid` );";

        public static string WriteAheadLogPragma = "PRAGMA journal_mode = WAL";

        public static string SynchronousNormalPragma = "PRAGMA synchronous = NORMAL";

        //--//
        public static string PlayHistoryInsert = $@"
            INSERT INTO `{SQLiteSchemaStrings.PLAYED_SONGS_TABLE_NAME}`(
	            `songid`,
	            `songname`,
	            `artistname`,
	            `albumname`,
	            `songLength`,
	            `albumYear`,
	            `arrangements`,
                `album_art`,
                `notes_hit`,
                `notes_missed`,
                `total_notes`,
                `max_streak`,
                `start_time`,
                `end_time`
            )
            VALUES (@songid,@songname,@artistname,@albumname,@songLength,@albumYear,@arrangements,@album_art,@notes_hit,@notes_missed,@total_notes,@max_streak,@start_time,@end_time);
            ";

        public static string PlayHistorySelectBySongId = $@"SELECT * FROM {SQLiteSchemaStrings.PLAYED_SONGS_TABLE_NAME} WHERE songid = @songid LIMIT 1";
        public static string PlayHistorySelectAll = $@"SELECT * FROM {SQLiteSchemaStrings.PLAYED_SONGS_TABLE_NAME}";
    }
}
