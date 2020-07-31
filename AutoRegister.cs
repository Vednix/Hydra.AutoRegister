using Hydra.Extensions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;
using TShockAPI.Hooks;

namespace Hydra.AutoRegister
{
    [ApiVersion(2, 1)]
    public class AutoRegister : TerrariaPlugin
    {
        public override Version Version => new Version(1, 0, 0, 0);
        public override string Name
        {
            get { return "Hydra.AutoRegister"; }
        }

        public override string Author
        {
            get { return "Vednix"; }
        }
        public AutoRegister(Main game) : base(game)
        {
            Order = 1;
        }
        internal static Config PConfig;
        public override void Initialize()
        {
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnGreetPlayer);
            ServerApi.Hooks.GamePostInitialize.Register(this, Config.OnPluginInitialize);
            GeneralHooks.ReloadEvent += Config.OnReloadEvent;
        }
        public static string CreatePassword(int length)
        {
            const string valid = "abcdefghjkpqrstuvwxyzABCDEFGHJKLPQRSTUVWXYZ23456789";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }
        public static void OnGreetPlayer(GreetPlayerEventArgs args)
        {
            var player = TShock.Players[args.Who];
            if (player == null || TShock.Users.GetUsers().Where(u => u != null && u.Name.ToLowerInvariant() == player.Name.ToLowerInvariant()).Count() >= 1 || !PConfig.EnableAutoRegister)
                return;
            Task.Run(() =>
            {
                System.Threading.Thread.Sleep(2000);
                if (PConfig.AnnounceNewAccount)
                    foreach (TSPlayer tsplayer in TShockB.Players.Where(p => p != null && p != player))
                    {
                        TSPlayerB.SendMessage(tsplayer.Index, DefaultMessage: tsplayer.TPlayer.Male ? $"yay! [c/FFD700:We have a new Terrarian with us! Be very welcome,] [c/00a0dd:{player.Name}]" : $"yay! [c/FFD700:We have a new Terrarian with us! Be very welcome,] [c/ed177a:{player.Name}]", Color.Magenta,
                                                              PortugueseMessage: tsplayer.TPlayer.Male ? $"Oba! [c/FFD700:Temos um novo Terrariano conosco! Seja muito bem vindo,] [c/00a0dd:{player.Name}]" : $"Oba! [c/FFD700:Temos uma nova Terrariana conosco! Seja muito bem vinda,] [c/ed177a:{player.Name}]",
                                                              SpanishMessage: tsplayer.TPlayer.Male ? $"Hurra! [c/FFD700:Tenemos un nuevo Terrario con nosotros! Sea muy bienvenido,] [c/00a0dd:{player.Name}]" : $"Hurra! [c/FFD700:Tenemos un nuevo Terrario con nosotros! Sea muy bienvenida,] [c/ed177a:{player.Name}]",
                                                              EnglishMessageIfNotDefault: tsplayer.TPlayer.Male ? $"yay! [c/FFD700:We have a new Terrarian with us! Be very welcome,] [c/00a0dd:{player.Name}]" : $"yay! [c/FFD700:We have a new Terrarian with us! Be very welcome,] [c/ed177a:{player.Name}]");
                    }

                string newPass = CreatePassword(6);
                Logger.doLogLang(DefaultMessage: $"[Hydra.AutoRegister] Creating a new account for: {player.Name}", Hydra.Config.DebugLevel.Info, Base.CurrentHydraLanguage,
                                 PortugueseMessage: $"[Hydra.AutoRegister] Criando uma nova conta para: {player.Name}",
                                 SpanishMessage: $"[Hydra.AutoRegister] Creando una nueva cuenta para el: {player.Name}",
                                 EnglishMessageIfNotDefault: $"[Hydra.AutoRegister] Creating a new account for: {player.Name}");

                try
                {
                    TShock.Users.AddUser(new User(
                        player.Name,
                        BCrypt.Net.BCrypt.HashPassword(newPass.Trim()),
                        player.UUID,
                        PConfig.SelectRegisterGroupAccordingGender ? (player.TPlayer.Male ? PConfig.DefaultGroup : PConfig.FemaleDefaultGroup) : PConfig.DefaultGroup,
                        DateTime.UtcNow.ToString("s"),
                        DateTime.UtcNow.ToString("s"),
                        ""));

                    //auto login
                    User user = TShock.Users.GetUserByName(player.Name);

                    if (user.VerifyPassword(newPass))
                    {
                        player.PlayerData = TShock.CharacterDB.GetPlayerData(player, user.ID);

                        var group = TShock.Utils.GetGroup(user.Group);

                        player.Group = group;
                        player.tempGroup = null;
                        player.User = user;
                        player.IsLoggedIn = true;
                        player.IgnoreActionsForInventory = "none";

                        if (Main.ServerSideCharacter)
                        {
                            if (player.HasPermission(TShockAPI.Permissions.bypassssc))
                            {
                                player.PlayerData.CopyCharacter(player);
                                TShock.CharacterDB.InsertPlayerData(player);
                            }
                            player.PlayerData.RestoreCharacter(player);
                        }
                        player.LoginFailsBySsi = false;

                        if (player.HasPermission(TShockAPI.Permissions.ignorestackhackdetection))
                            player.IgnoreActionsForCheating = "none";

                        if (player.HasPermission(TShockAPI.Permissions.usebanneditem))
                            player.IgnoreActionsForDisabledArmor = "none";

                        player.LoginHarassed = false;
                        TShock.Users.SetUserUUID(user, player.UUID);

                        Logger.doLogLang(DefaultMessage: $"[Hydra.AutoRegister] '{player.Name}' was automatically authenticated with the new registered account.", Hydra.Config.DebugLevel.Info, Base.CurrentHydraLanguage,
                                         PortugueseMessage: $"[Hydra.AutoRegister] '{player.Name}' foi autenticado automaticamente com a nova conta cadastrada.",
                                         SpanishMessage: $"[Hydra.AutoRegister] '{player.Name}' se autenticó automáticamente con la nueva cuenta registrada.",
                                         EnglishMessageIfNotDefault: $"[Hydra.AutoRegister] '{player.Name}' was automatically authenticated with the new registered account.");

                        TSPlayerB.SendMessage(player.Index, DefaultMessage: "Hello! We saw that you are new so we already created your account!", Color.DeepPink,
                                                            PortugueseMessage: $"Olá! Vimos que você é {(player.TPlayer.Male ? "novo" : "nova")} então já criamos a sua conta!",
                                                            SpanishMessage: $"Hola! Vimos que eres {(player.TPlayer.Male ? "nuevo" : "nueva")}, así que ya creamos tu cuenta",
                                                            EnglishMessageIfNotDefault: "Hello! We saw that you are new so we already created your account!");

                        TSPlayerB.SendMessage(player.Index, DefaultMessage: $"Your password is [c/ffa500:{newPass}], change it with [c/ffd700:/password].", Color.Red,
                                                            PortugueseMessage: $"Sua senha é [c/ffa500:{newPass}], altere-a com [c/ffd700:/senha].",
                                                            SpanishMessage: $"Tu contraseña es [c/ffa500:{newPass}], cambiarlo con [c/ffd700:contraseña].",
                                                            EnglishMessageIfNotDefault: $"Your password is [c/ffa500:{newPass}], change it with [c/ffd700:/password].");

                        PlayerHooks.OnPlayerPostLogin(player);
                    }
                } catch (Exception ex) { Logger.doLog(ex.ToString(), Hydra.Config.DebugLevel.Critical); }
            });
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnGreetPlayer);
                ServerApi.Hooks.GamePostInitialize.Deregister(this, Config.OnPluginInitialize);
                GeneralHooks.ReloadEvent -= Config.OnReloadEvent;
            }
            base.Dispose(disposing);
        }
    }
}
