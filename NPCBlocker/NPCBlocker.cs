using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TerrariaApi.Server;
using TShockAPI;
using Terraria;
using Newtonsoft.Json;

namespace NPCBlocker
{
	[ApiVersion(1, 17)]
	public class NPCBlocker : TerrariaPlugin
	{
		private List<int> blockedNPC = new List<int>();

		public override Version Version
		{
			get { return Assembly.GetExecutingAssembly().GetName().Version; }
		}

		public override string Name
		{
			get { return "NPC Blocker"; }
		}

		public override string Author
		{
			get { return "Zack Piispanen & Simon311"; }
		}

		public override string Description
		{
			get { return "Blocks npcs from being spawned."; }
		}

		public NPCBlocker(Main game)
			: base(game)
		{
			Order = 4;
		}

		public override void Initialize()
		{
			TShockAPI.Commands.ChatCommands.Add(new Command("resnpc", AddNPC, "blacknpc"));
			TShockAPI.Commands.ChatCommands.Add(new Command("resnpc", DelNPC, "whitenpc"));
			TShockAPI.Commands.ChatCommands.Add(new Command("resnpc", PrintNPC, "printnpc"));
			ServerApi.Hooks.NpcSpawn.Register(this, OnSpawn, 100);
			ServerApi.Hooks.NpcTransform.Register(this, OnTransform, 100);
			LoadConfig();
		}

		public string ConfigPath
		{
			get
			{
				return Path.Combine(TShock.SavePath, "NPCBlocker.json");
			}
		}

		private void LoadConfig()
		{
			if (!File.Exists(ConfigPath)) File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(new List<int> { 24, 48, 59, 60, 61, 69, 94 }, Formatting.Indented));
			blockedNPC = JsonConvert.DeserializeObject<List<int>>(File.ReadAllText(ConfigPath));
		}

		private void SaveConfig()
		{
			File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(blockedNPC, Formatting.Indented));
		}

		private void AddNPC(CommandArgs args)
		{
			if (args.Parameters.Count < 1)
			{
				args.Player.SendMessage("You must specify an NPC ID to add.", Color.Red);
				return;
			}

			int ID;

			if (!int.TryParse(args.Parameters[0], out ID))
			{
				args.Player.SendErrorMessage(String.Format("'{0}' is not a valid number.", args.Parameters[0]));
				return;
			}

			if (blockedNPC.Contains(ID))
			{
				args.Player.SendErrorMessage(String.Format("NPC ID {0} is already blacklisted!", args.Parameters[0]));
				return;
			}

			blockedNPC.Add(ID);
			args.Player.SendSuccessMessage(string.Format("NPC ID {0} succesfully blacklisted.", ID));
			SaveConfig();
		}

		private void DelNPC(CommandArgs args)
		{
			if (args.Parameters.Count < 1)
			{
				args.Player.SendMessage("You must specify an NPC ID to un-blacklist.", Color.Red);
				return;
			}

			int ID;

			if (!int.TryParse(args.Parameters[0], out ID))
			{
				args.Player.SendErrorMessage(String.Format("'{0}' is not a valid number.", args.Parameters[0]));
				return;
			}

			if (!blockedNPC.Contains(ID))
			{
				args.Player.SendErrorMessage(String.Format("NPC ID {0} is not blacklisted!", args.Parameters[0]));
				return;
			}

			blockedNPC.Remove(ID);
			args.Player.SendSuccessMessage(String.Format("NPC ID {0} succesfully un-blacklisted.", ID));
			SaveConfig();
		}

		private void PrintNPC(CommandArgs args)
		{
			args.Player.SendInfoMessage("Banned NPC IDs: " + string.Join(", ", blockedNPC) + ".");
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ServerApi.Hooks.NpcSpawn.Deregister(this, OnSpawn);
				ServerApi.Hooks.NpcTransform.Deregister(this, OnTransform);
			}

			base.Dispose(disposing);
		}

		private void OnSpawn( NpcSpawnEventArgs args)
		{
			if (args.Handled)
				return;

			if (blockedNPC.Contains(Main.npc[args.NpcId].netID))
			{
				args.Handled = true;
				Main.npc[args.NpcId].active = false;
				args.NpcId = 200;
			}
		}

		private void OnTransform(NpcTransformationEventArgs args)
		{
			if (args.Handled)
				return;

			if (blockedNPC.Contains(Main.npc[args.NpcId].netID))
				Main.npc[args.NpcId].active = false;
		}
	}
}
