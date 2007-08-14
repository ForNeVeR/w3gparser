using System.IO;
using Deerchao.War3Share.Utility;

namespace Deerchao.War3Share.W3gParser
{
    public abstract class W3gPlayerAction
    {
        byte actionId;

        public int ActionsCountForAPM(APMContext context)
        {
            //always ensure ResumeGameAction got a chance to modify the IsPaused field.
            bool countForAPM = CountForAPM(context);
            bool isPaused = context.IsPaused;

            if (isPaused)
                return 0;
            if (countForAPM)
                return 1;
            return 0;
        }

        protected virtual bool CountForAPM(APMContext context)
        {
            return true;
        }

        internal virtual void WriteBytes(BinaryWriter writer)
        {
            writer.Write(actionId);
        }

        internal static W3gPlayerAction Parse(BinaryReader reader, out int used)
        {
            used = 1;
            byte id = reader.ReadByte();
            W3gPlayerAction action = null;
            switch (id)
            {
                case 0x01:
                    action = PauseGameAction.Parse(reader, ref used);
                    break;
                case 0x02:
                    action = ResumeGameAction.Parse(reader, ref used);
                    break;
                case 0x03:
                    action = SetSpeedAction.Parse(reader, ref used);
                    break;
                case 0x04:
                    action = IncreaseSpeedAction.Parse(reader, ref used);
                    break;
                case 0x05:
                    action = DecreaseSpeedAction.Parse(reader, ref used);
                    break;
                case 0x06:
                    action = SaveGameAction.Parse(reader, ref used);
                    break;
                case 0x07:
                    action = GameSavedAction.Parse(reader, ref used);
                    break;
                case 0x10:
                    action = UnitAbilityActionA.Parse(reader, ref used);
                    break;
                case 0x11:
                    action = UnitAbilityActionB.Parse(reader, ref used);
                    break;
                case 0x12:
                    action = UnitAbilityActionC.Parse(reader, ref used);
                    break;
                case 0x13:
                    action = GiveItemAction.Parse(reader, ref used);
                    break;
                case 0x14:
                    action = UnitAbilityActionD.Parse(reader, ref used);
                    break;
                case 0x16:
                    action = ChangeSelectionAction.Parse(reader, ref used);
                    break;
                case 0x17:
                    action = CreateGroupAction.Parse(reader, ref used);
                    break;
                case 0x18:
                    action = SelectGroupAction.Parse(reader, ref used);
                    break;
                case 0x19:
                    action = SelectSubGroupAction.Parse(reader, ref used);
                    break;
                case 0x1A:
                    action = PreSelectSubGroupAction.Parse(reader, ref used);
                    break;
                case 0x1B:
                    action = UnknownActionA.Parse(reader, ref used);
                    break;
                case 0x1C:
                    action = SelectGroundItemAction.Parse(reader, ref used);
                    break;
                case 0x1D:
                    action = CancelHeroRevivalAction.Parse(reader, ref used);
                    break;
                case 0x1E:
                    action = CancelUnitTraintingAction.Parse(reader, ref used);
                    break;
                case 0x21:
                    action = UnknownActionB.Parse(reader, ref used);
                    break;
                case 0x20:
                case 0x22:
                case 0x23:
                case 0x24:
                case 0x25:
                case 0x26:
                case 0x29:
                case 0x2A:
                case 0x2B:
                case 0x2C:
                case 0x2F:
                case 0x30:
                case 0x31:
                case 0x32:
                    action = CheatActionA.Parse(reader, ref used);
                    break;
                case 0x27:
                case 0x28:
                case 0x2D:
                    action = CheatActionB.Parse(reader, ref used);
                    break;
                case 0x2E:
                    action = CheatActionC.Parse(reader, ref used);
                    break;
                case 0x50:
                    action = ChangeAllyOptionAction.Parse(reader, ref used);
                    break;
                case 0x51:
                    action = TransferResourceAction.Parse(reader, ref used);
                    break;
                case 0x60:
                    action = TriggerChatAction.Parse(reader, ref used);
                    break;
                case 0x61:
                    action = PressEscAction.Parse(reader, ref used);
                    break;
                case 0x62:
                    action = ScenarioTriggerAction.Parse(reader, ref used);
                    break;
                case 0x66:
                    action = BeginChooseHeroSkillAction.Parse(reader, ref used);
                    break;
                case 0x67:
                    action = BeginChooseBuildingAction.Parse(reader, ref used);
                    break;
                case 0x68:
                    action = PingMiniMapAction.Parse(reader, ref used);
                    break;
                case 0x69:
                    action = ContinueGameActionB.Parse(reader, ref used);
                    break;
                case 0x6A:
                    action = ContinueGameActionA.Parse(reader, ref used);
                    break;
                case 0x75:
                    action = UnknownActionC.Parse(reader, ref used);
                    break;
            }
            if (action != null)
                action.actionId = id;
            return action;
        }
    }

    public class APMContext
    {
        public APMContext()
        {
            isPaused = false;
            wasDeselect = false;
        }

        public void Refresh()
        {
            wasDeselect = false;
        }

        bool isPaused;
        bool wasDeselect;

        public bool IsPaused
        {
            get { return isPaused; }
            set { isPaused = value; }
        }

        public bool WasDeselect
        {
            get { return wasDeselect; }
            set { wasDeselect = value; }
        }
    }

    public class PauseGameAction : W3gPlayerAction
    {
        internal static PauseGameAction Parse(BinaryReader reader, ref int used)
        {
            return new PauseGameAction();
        }

        protected override bool CountForAPM(APMContext context)
        {
            context.IsPaused = true;
            return false;
        }
    }

    public class ResumeGameAction : W3gPlayerAction
    {
        internal static ResumeGameAction Parse(BinaryReader reader, ref int used)
        {
            return new ResumeGameAction();
        }

        protected override bool CountForAPM(APMContext context)
        {
            context.IsPaused = false;
            return false;
        }
    }

    public class SetSpeedAction : W3gPlayerAction
    {
        GameSpeed speed;

        internal static SetSpeedAction Parse(BinaryReader reader, ref int used)
        {
            SetSpeedAction action = new SetSpeedAction();
            action.speed = (GameSpeed)reader.ReadByte();
            used++;
            return action;
        }

        protected override bool CountForAPM(APMContext context)
        {
            return false;
        }

        internal override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);
            writer.Write((byte)speed);
        }
    }

    public class IncreaseSpeedAction : W3gPlayerAction
    {
        internal static IncreaseSpeedAction Parse(BinaryReader reader, ref int used)
        {
            return new IncreaseSpeedAction();
        }

        protected override bool CountForAPM(APMContext context)
        {
            return false;
        }
    }

    public class DecreaseSpeedAction : W3gPlayerAction
    {
        internal static DecreaseSpeedAction Parse(BinaryReader reader, ref int used)
        {
            return new DecreaseSpeedAction();
        }

        protected override bool CountForAPM(APMContext context)
        {
            return false;
        }
    }

    public class SaveGameAction : W3gPlayerAction
    {
        string gameName;

        internal static SaveGameAction Parse(BinaryReader reader, ref int used)
        {
            SaveGameAction action = new SaveGameAction();
            action.gameName = BinaryHelper.ReadSZString(reader);
            used += action.gameName.Length + 1;
            return action;
        }

        protected override bool CountForAPM(APMContext context)
        {
            return false;
        }

        internal override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);
            writer.Write(gameName);
        }
    }

    public class GameSavedAction : W3gPlayerAction
    {
        private int unknown;

        internal static GameSavedAction Parse(BinaryReader reader, ref int used)
        {
            GameSavedAction action = new GameSavedAction();
            action.unknown = reader.ReadInt32();
            used += 4;
            return action;
        }

        protected override bool CountForAPM(APMContext context)
        {
            return false;
        }

        internal override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);
            writer.Write(unknown);
        }
    }

    public class UnitAbilityActionA : W3gPlayerAction
    {
        short abilityFlags;
        int itemId;
        int unknownA;
        int unknownB;

        internal static UnitAbilityActionA Parse(BinaryReader reader, ref int used)
        {
            UnitAbilityActionA action = new UnitAbilityActionA();
            action.abilityFlags = reader.ReadInt16();
            action.itemId = reader.ReadInt32();
            action.unknownA = reader.ReadInt32();
            action.unknownB = reader.ReadInt32();
            used += 14;
            return action;
        }

        internal override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);
            writer.Write(abilityFlags);
            writer.Write(itemId);
            writer.Write(unknownA);
            writer.Write(unknownB);
        }
    }

    public class UnitAbilityActionB : W3gPlayerAction
    {
        short abilityFlags;
        int itemId;
        int unknownA;
        int unknownB;
        int x;
        int y;

        internal static UnitAbilityActionB Parse(BinaryReader reader, ref int used)
        {
            UnitAbilityActionB action = new UnitAbilityActionB();
            action.abilityFlags = reader.ReadInt16();
            action.itemId = reader.ReadInt32();
            action.unknownA = reader.ReadInt32();
            action.unknownB = reader.ReadInt32();
            action.x = reader.ReadInt32();
            action.y = reader.ReadInt32();
            used += 22;
            return action;
        }

        internal override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);
            writer.Write(abilityFlags);
            writer.Write(itemId);
            writer.Write(unknownA);
            writer.Write(unknownB);
            writer.Write(x);
            writer.Write(y);
        }
    }

    public class UnitAbilityActionC : W3gPlayerAction
    {
        short abilityFlags;
        int itemId;
        int unknownA;
        int unknownB;
        int x;
        int y;
        int objectId1;
        int objectId2;

        internal static UnitAbilityActionC Parse(BinaryReader reader, ref int used)
        {
            UnitAbilityActionC action = new UnitAbilityActionC();
            action.abilityFlags = reader.ReadInt16();
            action.itemId = reader.ReadInt32();
            action.unknownA = reader.ReadInt32();
            action.unknownB = reader.ReadInt32();
            action.x = reader.ReadInt32();
            action.y = reader.ReadInt32();
            action.objectId1 = reader.ReadInt32();
            action.objectId2 = reader.ReadInt32();
            used += 30;
            return action;
        }

        internal override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);
            writer.Write(abilityFlags);
            writer.Write(itemId);
            writer.Write(unknownA);
            writer.Write(unknownB);
            writer.Write(x);
            writer.Write(y);
            writer.Write(objectId1);
            writer.Write(objectId2);
        }
    }

    public class GiveItemAction : W3gPlayerAction
    {
        short abilityFlags;
        int itemId;
        int unknownA;
        int unknownB;
        int x;
        int y;
        int objectId1;
        int objectId2;
        int itemId1;
        int itemId2;

        internal static GiveItemAction Parse(BinaryReader reader, ref int used)
        {
            GiveItemAction action = new GiveItemAction();
            action.abilityFlags = reader.ReadInt16();
            action.itemId = reader.ReadInt32();
            action.unknownA = reader.ReadInt32();
            action.unknownB = reader.ReadInt32();
            action.x = reader.ReadInt32();
            action.y = reader.ReadInt32();
            action.objectId1 = reader.ReadInt32();
            action.objectId2 = reader.ReadInt32();
            action.itemId1 = reader.ReadInt32();
            action.itemId2 = reader.ReadInt32();
            used += 38;
            return action;
        }

        internal override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);
            writer.Write(abilityFlags);
            writer.Write(itemId);
            writer.Write(unknownA);
            writer.Write(unknownB);
            writer.Write(x);
            writer.Write(y);
            writer.Write(objectId1);
            writer.Write(objectId2);
            writer.Write(itemId1);
            writer.Write(itemId2);
        }
    }

    public class UnitAbilityActionD : W3gPlayerAction
    {
        short abilityFlags;
        int itemId;
        int unknownA;
        int unknownB;
        int x;
        int y;
        int itemId2;
        byte[] unknown;
        int x2;
        int y2;

        internal static UnitAbilityActionD Parse(BinaryReader reader, ref int used)
        {
            UnitAbilityActionD action = new UnitAbilityActionD();
            action.abilityFlags = reader.ReadInt16();
            action.itemId = reader.ReadInt32();
            action.unknownA = reader.ReadInt32();
            action.unknownB = reader.ReadInt32();
            action.x = reader.ReadInt32();
            action.y = reader.ReadInt32();
            action.itemId2 = reader.ReadInt32();
            action.unknown = reader.ReadBytes(9);
            action.x2 = reader.ReadInt32();
            action.y2 = reader.ReadInt32();
            used += 43;
            return action;
        }
        internal override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);
            writer.Write(abilityFlags);
            writer.Write(itemId);
            writer.Write(unknownA);
            writer.Write(unknownB);
            writer.Write(x);
            writer.Write(y);
            writer.Write(itemId2);
            writer.Write(unknown);
            writer.Write(x2);
            writer.Write(y2);
        }
    }

    public class ChangeSelectionAction : W3gPlayerAction
    {
        SelectMode mode;
        short unitCount;
        int[] objectIds;

        public SelectMode Mode
        {
            get { return mode; }
        }

        internal static ChangeSelectionAction Parse(BinaryReader reader, ref int used)
        {
            ChangeSelectionAction action = new ChangeSelectionAction();
            action.mode = (SelectMode)reader.ReadByte();
            action.unitCount = reader.ReadInt16();
            action.objectIds = new int[action.unitCount * 2];
            for (int i = 0; i < action.unitCount; i++)
            {
                action.objectIds[2 * i] = reader.ReadInt32();
                action.objectIds[2 * i + 1] = reader.ReadInt32();
            }
            used += 3 + action.unitCount * 8;
            return action;
        }

        internal override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);
            writer.Write((byte)mode);
            writer.Write(unitCount);
            foreach (int id in objectIds)
                writer.Write(id);
        }

        protected override bool CountForAPM(APMContext context)
        {
            bool result;

            if (mode == SelectMode.Deselect)
                result = true;
            else
                result = !context.WasDeselect;

            if (Mode == SelectMode.Deselect)
                context.WasDeselect = true;
            else
                context.WasDeselect = false;

            return result;
        }
    }

    public enum SelectMode : byte
    {
        Select = 1,
        Deselect = 2
    }

    public class CreateGroupAction : W3gPlayerAction
    {
        byte groupNo;
        short unitCount;
        int[] unitIds;

        internal static CreateGroupAction Parse(BinaryReader reader, ref int used)
        {
            CreateGroupAction action = new CreateGroupAction();
            action.groupNo = reader.ReadByte();
            action.unitCount = reader.ReadInt16();
            action.unitIds = new int[action.unitCount * 2];
            for (int i = 0; i < action.unitCount; i++)
            {
                action.unitIds[2 * i] = reader.ReadInt32();
                action.unitIds[2 * i + 1] = reader.ReadInt32();
            }
            used += 3 + action.unitCount * 8;
            return action;
        }

        internal override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);
            writer.Write(groupNo);
            writer.Write(unitCount);
            foreach (int id in unitIds)
                writer.Write(id);
        }
    }

    public class SelectGroupAction : W3gPlayerAction
    {
        byte groupNo;
        byte unknown;

        internal static SelectGroupAction Parse(BinaryReader reader, ref int used)
        {
            SelectGroupAction action = new SelectGroupAction();
            action.groupNo = reader.ReadByte();
            action.unknown = reader.ReadByte();
            used += 2;
            return action;
        }

        internal override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);
            writer.Write(groupNo);
            writer.Write(unknown);
        }
    }

    public class SelectSubGroupAction : W3gPlayerAction
    {
        int itemId;
        int objectId1;
        int objectId2;

        internal static SelectSubGroupAction Parse(BinaryReader reader, ref int used)
        {
            SelectSubGroupAction action = new SelectSubGroupAction();
            action.itemId = reader.ReadInt32();
            action.objectId1 = reader.ReadInt32();
            action.objectId2 = reader.ReadInt32();
            used += 12;
            return action;
        }

        protected override bool CountForAPM(APMContext context)
        {
            return false;
        }

        internal override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);
            writer.Write(itemId);
            writer.Write(objectId1);
            writer.Write(objectId2);
        }
    }

    public class PreSelectSubGroupAction : W3gPlayerAction
    {
        internal static PreSelectSubGroupAction Parse(BinaryReader reader, ref int used)
        {
            return new PreSelectSubGroupAction();
        }

        protected override bool CountForAPM(APMContext context)
        {
            return false;
        }
    }

    public class UnknownActionA : W3gPlayerAction
    {
        private byte unknown;
        private int objectId1;
        private int objectId2;

        internal static UnknownActionA Parse(BinaryReader reader, ref int used)
        {
            UnknownActionA action = new UnknownActionA();
            action.unknown = reader.ReadByte();
            action.objectId1 = reader.ReadInt32();
            action.objectId2 = reader.ReadInt32();

            used += 9;
            return action;
        }

        internal override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);
            writer.Write(unknown);
            writer.Write(objectId1);
            writer.Write(objectId2);
        }

        protected override bool CountForAPM(APMContext context)
        {
            return false;
        }
    }

    public class SelectGroundItemAction : W3gPlayerAction
    {
        byte unknown;
        int objectId1;
        int objectId2;

        internal static SelectGroundItemAction Parse(BinaryReader reader, ref int used)
        {
            SelectGroundItemAction action = new SelectGroundItemAction();
            action.unknown = reader.ReadByte();
            action.objectId1 = reader.ReadInt32();
            action.objectId2 = reader.ReadInt32();
            used += 9;
            return action;
        }
        internal override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);
            writer.Write(unknown);
            writer.Write(objectId1);
            writer.Write(objectId2);
        }
    }

    public class CancelHeroRevivalAction : W3gPlayerAction
    {
        int unitId1;
        int unitId2;

        internal static CancelHeroRevivalAction Parse(BinaryReader reader, ref int used)
        {
            CancelHeroRevivalAction action = new CancelHeroRevivalAction();
            action.unitId1 = reader.ReadInt32();
            action.unitId2 = reader.ReadInt32();
            used += 8;
            return action;
        }

        internal override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);
            writer.Write(unitId1);
            writer.Write(unitId2);
        }
    }

    public class CancelUnitTraintingAction : W3gPlayerAction
    {
        byte slotNo;
        int itemId;

        internal static CancelUnitTraintingAction Parse(BinaryReader reader, ref int used)
        {
            CancelUnitTraintingAction action = new CancelUnitTraintingAction();
            action.slotNo = reader.ReadByte();
            action.itemId = reader.ReadInt32();
            used += 5;
            return action;
        }

        internal override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);
            writer.Write(slotNo);
            writer.Write(itemId);
        }
    }

    public class UnknownActionB : W3gPlayerAction
    {
        private int unknownA;
        private int unknownB;

        internal static UnknownActionB Parse(BinaryReader reader, ref int used)
        {
            UnknownActionB action = new UnknownActionB();
            action.unknownA = reader.ReadInt32();
            action.unknownB = reader.ReadInt32();
            used += 8;
            return action;
        }

        internal override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);
            writer.Write(unknownA);
            writer.Write(unknownB);
        }

        protected override bool CountForAPM(APMContext context)
        {
            return false;
        }
    }

    public class CheatActionA : W3gPlayerAction
    {
        internal static CheatActionA Parse(BinaryReader reader, ref int used)
        {
            CheatActionA action = new CheatActionA();
            return action;
        }

        protected override bool CountForAPM(APMContext context)
        {
            return false;
        }
    }

    public class CheatActionB : W3gPlayerAction
    {
        byte unknown;
        int amount;

        internal static CheatActionB Parse(BinaryReader reader, ref int used)
        {
            CheatActionB action = new CheatActionB();
            action.unknown = reader.ReadByte();
            action.amount = reader.ReadInt32();
            used += 5;
            return action;
        }

        protected override bool CountForAPM(APMContext context)
        {
            return false;
        }

        internal override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);
            writer.Write(unknown);
            writer.Write(amount);
        }
    }

    public class CheatActionC : W3gPlayerAction
    {
        float time;

        internal static CheatActionC Parse(BinaryReader reader, ref int used)
        {
            CheatActionC action = new CheatActionC();
            action.time = reader.ReadSingle();
            used += 4;
            return action;
        }

        protected override bool CountForAPM(APMContext context)
        {
            return false;
        }

        internal override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);
            writer.Write(time);
        }
    }

    public class ChangeAllyOptionAction : W3gPlayerAction
    {
        byte playerSlotNo;
        int flags;

        internal static ChangeAllyOptionAction Parse(BinaryReader reader, ref int used)
        {
            ChangeAllyOptionAction action = new ChangeAllyOptionAction();
            action.playerSlotNo = reader.ReadByte();
            action.flags = reader.ReadInt32();
            used += 5;
            return action;
        }

        protected override bool CountForAPM(APMContext context)
        {
            return false;
        }

        internal override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);
            writer.Write(playerSlotNo);
            writer.Write(flags);
        }
    }

    public class TransferResourceAction : W3gPlayerAction
    {
        byte playerSlotNo;
        int gold;
        int lumber;

        internal static TransferResourceAction Parse(BinaryReader reader, ref int used)
        {
            TransferResourceAction action = new TransferResourceAction();
            action.playerSlotNo = reader.ReadByte();
            action.gold = reader.ReadInt32();
            action.lumber = reader.ReadInt32();
            used += 9;
            return action;
        }

        protected override bool CountForAPM(APMContext context)
        {
            return false;
        }

        internal override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);
            writer.Write(playerSlotNo);
            writer.Write(gold);
            writer.Write(lumber);
        }
    }

    public class TriggerChatAction : W3gPlayerAction
    {
        int unknownA;
        int unknownB;
        string triggerName;

        internal static TriggerChatAction Parse(BinaryReader reader, ref int used)
        {
            TriggerChatAction action = new TriggerChatAction();
            action.unknownA = reader.ReadInt32();
            action.unknownB = reader.ReadInt32();
            action.triggerName = BinaryHelper.ReadSZString(reader);
            used += 8 + action.triggerName.Length + 1;
            return action;
        }

        protected override bool CountForAPM(APMContext context)
        {
            return false;
        }

        internal override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);
            writer.Write(unknownA);
            writer.Write(unknownB);
            writer.Write(triggerName);
        }

    }

    public class PressEscAction : W3gPlayerAction
    {
        internal static PressEscAction Parse(BinaryReader reader, ref int used)
        {
            return new PressEscAction();
        }
    }

    public class ScenarioTriggerAction : W3gPlayerAction
    {
        int unknownA;
        int unknownB;
        int unknownC;

        internal static ScenarioTriggerAction Parse(BinaryReader reader, ref int used)
        {
            ScenarioTriggerAction action = new ScenarioTriggerAction();
            action.unknownA = reader.ReadInt32();
            action.unknownB = reader.ReadInt32();
            action.unknownC = reader.ReadInt32();
            used += 12;
            return action;
        }

        protected override bool CountForAPM(APMContext context)
        {
            return false;
        }

        internal override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);
            writer.Write(unknownA);
            writer.Write(unknownB);
            writer.Write(unknownC);
        }
    }

    public class BeginChooseHeroSkillAction : W3gPlayerAction
    {
        internal static BeginChooseHeroSkillAction Parse(BinaryReader reader, ref int used)
        {
            return new BeginChooseHeroSkillAction();
        }
    }

    public class BeginChooseBuildingAction : W3gPlayerAction
    {
        internal static BeginChooseBuildingAction Parse(BinaryReader reader, ref int used)
        {
            return new BeginChooseBuildingAction();
        }
    }

    public class PingMiniMapAction : W3gPlayerAction
    {
        int x;
        int y;
        int unknown;

        internal static PingMiniMapAction Parse(BinaryReader reader, ref int used)
        {
            PingMiniMapAction action = new PingMiniMapAction();
            action.x = reader.ReadInt32();
            action.y = reader.ReadInt32();
            action.unknown = reader.ReadInt32();
            used += 12;
            return action;
        }

        protected override bool CountForAPM(APMContext context)
        {
            return false;
        }

        internal override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);
            writer.Write(x);
            writer.Write(y);
            writer.Write(unknown);
        }
    }

    public class ContinueGameActionB : W3gPlayerAction
    {
        int unknownC;
        int unknownD;
        int unknownA;
        int unknownB;

        internal static ContinueGameActionB Parse(BinaryReader reader, ref int used)
        {
            ContinueGameActionB action = new ContinueGameActionB();
            action.unknownC = reader.ReadInt32();
            action.unknownD = reader.ReadInt32();
            action.unknownA = reader.ReadInt32();
            action.unknownB = reader.ReadInt32();
            used += 16;
            return action;
        }

        protected override bool CountForAPM(APMContext context)
        {
            return false;
        }

        internal override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);
            writer.Write(unknownC);
            writer.Write(unknownD);
            writer.Write(unknownB);
            writer.Write(unknownA);
        }
    }

    public class ContinueGameActionA : W3gPlayerAction
    {
        int unknownA;
        int unknownB;
        int unknownC;
        int unknownD;

        internal static ContinueGameActionA Parse(BinaryReader reader, ref int used)
        {
            ContinueGameActionA action = new ContinueGameActionA();
            action.unknownA = reader.ReadInt32();
            action.unknownB = reader.ReadInt32();
            action.unknownC = reader.ReadInt32();
            action.unknownD = reader.ReadInt32();
            used += 16;
            return action;
        }

        protected override bool CountForAPM(APMContext context)
        {
            return false;
        }

        internal override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);
            writer.Write(unknownA);
            writer.Write(unknownB);
            writer.Write(unknownC);
            writer.Write(unknownD);
        }
    }

    public class UnknownActionC : W3gPlayerAction
    {
        byte unknown;

        internal static UnknownActionC Parse(BinaryReader reader, ref int used)
        {
            UnknownActionC action = new UnknownActionC();
            action.unknown = reader.ReadByte();
            used += 1;
            return action;
        }

        protected override bool CountForAPM(APMContext context)
        {
            return false;
        }

        internal override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);
            writer.Write(unknown);
        }
    }
}
