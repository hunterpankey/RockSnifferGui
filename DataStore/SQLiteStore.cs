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

namespace RockSnifferGui.DataStore
{
    public class SQLiteStore
    {
        public const string DB_FILE_NAME = "userdata.sqlite";
        public const string PLAYED_SONGS_TABLE_NAME = "playInstances";
        private SQLiteConnection Connection { get; set; }

        public SQLiteStore()
        {
            if (!File.Exists(SQLiteStore.DB_FILE_NAME))
            {
                SQLiteConnection.CreateFile(SQLiteStore.DB_FILE_NAME);
            }

            Connection = new SQLiteConnection($"Data Source={SQLiteStore.DB_FILE_NAME};");
            Connection.Open();

            CreateTables();
        }

        private void CreateTables()
        {
            var q = $@"
            CREATE TABLE IF NOT EXISTS `{SQLiteStore.PLAYED_SONGS_TABLE_NAME}` (
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

            //`psarcFile`         TEXT NOT NULL,
            //`psarcFileHash` 	TEXT NOT NULL,
            //`toolkit_version`	TEXT,
            //`toolkit_author`	TEXT,
            //`toolkit_package_version`	TEXT,
            //`toolkit_comment`	TEXT

            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = q;
                cmd.ExecuteNonQuery();
            }

            //q = "CREATE INDEX IF NOT EXISTS `filepath` ON `songs` (`psarcFile` );";

            //using (var cmd = Connection.CreateCommand())
            //{
            //    cmd.CommandText = q;
            //    cmd.ExecuteNonQuery();
            //}

            q = $"CREATE INDEX IF NOT EXISTS`songid` ON `{SQLiteStore.PLAYED_SONGS_TABLE_NAME}` (`songid` );";

            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = q;
                cmd.ExecuteNonQuery();
            }

            // Enable WAL mode, it is MUCH faster
            // It shouldn't have any downsides in this case
            q = "PRAGMA journal_mode = WAL";
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = q;
                cmd.ExecuteNonQuery();
            }

            q = "PRAGMA synchronous = NORMAL";
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = q;
                cmd.ExecuteNonQuery();
            }

            if (Logger.logCache)
            {
                Logger.Log("SQLite database initialised");
            }
        }

        public long Add(SongPlayInstance songPlayInstance)
        {
            long toReturn = 0;

            var q = $@"
            INSERT INTO `{SQLiteStore.PLAYED_SONGS_TABLE_NAME}`(
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

            SQLiteTransaction transaction = Connection.BeginTransaction();

            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = q;

                //cmd.Parameters.Add("@psarcFile", DbType.String);
                //cmd.Parameters.Add("@psarcFileHash", DbType.String);
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

                //cmd.Parameters.Add("@toolkit_version", DbType.String);
                //cmd.Parameters.Add("@toolkit_author", DbType.String);
                //cmd.Parameters.Add("@toolkit_package_version", DbType.String);
                //cmd.Parameters.Add("@toolkit_comment", DbType.String);

                //foreach (KeyValuePair<string, SongDetails> pair in songPlayInstances.)
                //{

                //cmd.Parameters["@psarcFile"].Value = filepath;
                //cmd.Parameters["@psarcFileHash"].Value = sd.psarcFileHash;
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

                cmd.Parameters["@album_art"].Value = null;

                if (songPlayInstance.SongDetails.albumArt != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        songPlayInstance.SongDetails.albumArt.Save(ms, ImageFormat.Png);

                        cmd.Parameters["@album_art"].Value = ms.ToArray();
                    }
                }

                cmd.ExecuteNonQuery();
                toReturn = Connection.LastInsertRowId;

                transaction.Commit();

                if (Logger.logCache)
                {
                    Logger.Log($"Song played {songPlayInstance.SongDetails.songName}/{songPlayInstance.SongDetails.songID}");
                }
                //}
            }

            return toReturn;
        }

        public SongPlayInstance Get(string SongID)
        {
            string q = $@"SELECT * FROM {SQLiteStore.PLAYED_SONGS_TABLE_NAME} WHERE songid = @songid LIMIT 1";

            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = q;
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

                        try
                        {
                            var blob = ReadField<byte[]>(reader, "album_art");

                            using (var ms = new MemoryStream(blob))
                            {
                                songDetails.albumArt = Image.FromStream(ms);
                            }
                        }
                        catch
                        {

                        }

                        var noteDetails = new LearnASongNoteData();

                        return new SongPlayInstance(songDetails, noteDetails);
                    }
                }
            }

            return null;
        }

        public List<SongPlayInstance> GetAll()
        {
            List<SongPlayInstance> toReturn = new List<SongPlayInstance>();

            string q = $@"SELECT * FROM {SQLiteStore.PLAYED_SONGS_TABLE_NAME}";

            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = q;

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

                        try
                        {
                            var blob = ReadField<byte[]>(reader, "album_art");

                            using (var ms = new MemoryStream(blob))
                            {
                                songDetails.albumArt = Image.FromStream(ms);
                            }
                        }
                        catch
                        {

                        }

                        var noteDetails = new LearnASongNoteData();
                        SongPlayInstance toAdd = new SongPlayInstance(songDetails, noteDetails, ReadField<DateTime>(reader, "start_time"), ReadField<DateTime>(reader, "end_time"));
                        toReturn.Add(toAdd);
                    }
                }
            }

            return toReturn;
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

            var q = $@"
            INSERT INTO `{SQLiteStore.PLAYED_SONGS_TABLE_NAME}`(
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

            SQLiteTransaction transaction = Connection.BeginTransaction();

            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = q;

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
                // album art handled below
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
}
