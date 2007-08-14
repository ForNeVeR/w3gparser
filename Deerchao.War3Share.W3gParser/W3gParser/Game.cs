using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Deerchao.War3Share.Utility;

namespace Deerchao.War3Share.W3gParser
{
    public class W3gGame
    {
        string gameName;
        W3gGameSetting setting;
        uint playerCount;
        uint gameType;
        uint languageId;

        public W3gGameSetting Setting
        {
            get { return setting; }
        }

        public string GameName
        {
            get { return gameName; }
            set { gameName = value; }
        }

        public static W3gGame Parse(BinaryReader reader)
        {
            W3gGame g = new W3gGame();

            g.gameName = BinaryHelper.ReadSZString(reader);
            reader.ReadByte();

            g.setting = W3gGameSetting.Parse(reader);

            g.playerCount = reader.ReadUInt32();
            g.gameType = reader.ReadUInt32();
            g.languageId = reader.ReadUInt32();

            return g;
        }
    }
}
#region 4.0 [Decompressed data]
//===============================================================================

//Decompressed data is a collection of data items that appear back to back in
//the stream. The offsets for these items vary depending on the size of every
//single item.

//This section describes the records that always appear at the beginning of
//a replay data stream. They hold information about settings and players right
//before the start of the game. Data about the game in progress is described
//in section 5.

//The order of the start up items is as follows:

// # |   Size   | Name
//---+----------+--------------------------
// 1 |   4 byte | Unknown (0x00000110 - another record id?)
// 2 | variable | PlayerRecord (see 4.1)
// 3 | variable | GameName (null terminated string) (see 4.2)
// 4 |   1 byte | Nullbyte
// 5 | variable | Encoded String (null terminated) (see 4.3)
//   |          |  - GameSettings (see 4.4)
//   |          |  - Map&CreatorName (see 4.5)
// 6 |   4 byte | PlayerCount (see 4.6)
// 7 |   4 byte | GameType (see 4.7)
// 8 |   4 byte | LanguageID (see 4.8)
// 9 | variable | PlayerList (see 4.9)
//10 | variable | GameStartRecord (see 4.11)

//The following sections describe these items in detail.
//After the static items (as described above) there follow variable information
//organized in blocks that are described in section 5.

#endregion

#region 4.2 [GameName]
//- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

//This is a plain null terminated string reading the name of the game.

//Only in custom battle.net games you can name your game, otherwise the name
//is fixed:

//o For ladder games it reads "BNet".

//o For custom LAN games it holds a localized string including the game creators
//  player name.
//  Examples: a game created by the player 'Blue' results in:
//           "Blue's game" for the english version of Warcraft III
//           "Spiel von Blue" for the german version

//o For custom single player games it holds a localized fixed string.
//           "local game" for the english version
//           "Lokales Spiel" for the german version

//o Following is a list for all localized strings (encoded in plain ASCII) used
//  by WarCraft III patch version 1.06 and earlier
//  (see war3.mpq\UI\FrameDef\GlobalStrings.fdf: GAMENAME, LOCAL_GAME):

//  English        : "%s's Game"         : "local game"
//  Czech    (1029): "Hra %s"            : "Místn?hra"
//  German   (1031): "Spiel von %s"      : "Lokales Spiel"
//  Spanish  (1034): "Partida de %s"     : "Partida local"
//  French   (1036): "Partie de %s"      : "Partie locale"
//  Italian  (1040): "Partita di %s"     : "Partita locale"
//  Polish   (1045): "Gra (%s)"          : "Gra lokalna"

//  %s denotes the game creators player name.

//o The following list shows all localized strings (encoded in plain ASCII) used
//  by WarCraft III patch version 1.07 and later
//  (see war3.mpq\UI\FrameDef\GlobalStrings.fdf: GAMENAME, LOCAL_GAME):

//  German   (1031): "Lokales Spiel (%s)"  : "Lokales Spiel"

//  %s denotes the game creators player name.
#endregion

#region 4.6 [PlayerCount]
//- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

//4 bytes - num players or num slots
//  in Bnet games is the exact ## of players
//  in Custom games, is the ## of slots on the join game screen
//  in Single Player custom games is 12
#endregion

#region 4.7 [GameType]
//- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

//offset | size/type | Description
//-------+-----------+-----------------------------------------------------------
//0x0000 |  1 byte   | Game Type:
//       |           | (0x00 = unknown, just in a few  pre 1.03 custom games)
//       |           |  0x01 =   Ladder -> 1on1 or FFA
//                               Custom -> Scenario  (not 100% sure about this)
//       |           |  0x09 = Custom game
//       |           |  0x1D = Single player game
//       |           |  0x20 = Ladder Team game (AT or RT, 2on2/3on3/4on4)
//0x0001 |  1 byte   | PrivateFlag for custom games:
//       |           |  0x00 - if it is a public LAN/Battle.net game
//       |           |  0x08 - if it is a private Battle.net game
//0x0002 |  1 word   | unknown (always 0x0000 so far)

//TODO:
//  values in patch >=1.07:
//    01 00 00 00 : ladder 1on1 / custom scenario
//    20 00 00 00 : ladder team
//    09 00 00 00 : custom game
//    09 A8 12 00 : custom game
//    09 A0 12 00 : custom game
//    09 A8 42 00 : custom game
//    09 A8 14 00 : custom game
//    09 A0 14 00 : custom game
//    01 40 13 00 : custom game
//    09 A0 42 00 : custom game
//    09 A8 44 00 : custon game

#endregion

#region 4.8 [LanguageID]
//- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
//4 byte - (independent of realm, map, gametype !)
//Might be another checksum or encoded language.
//I found the following numbers in my replays:

//C4 F0 12 00 - patch 1.10 ger
//90 F1 12 00 - patch 1.06 ger
//90 F1 12 00 - patch 1.05 ger
//A0 F6 6D 00 - patch 1.04 ger
//24 F8 12 00 - patch 1.04 eng(?)
//A0 F6 6D 00 - patch 1.03 ger
//C0 F6 6D 00 - patch 1.02 ger

////TODO: Find out what this field is really about.
#endregion