﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using n3q.Tools;

namespace n3q.WebEx.Controllers
{
    [ApiController]
    public class ClientController : ControllerBase
    {
        public ICallbackLogger Log { get; set; }
        public WebExConfigDefinition Config { get; set; }

        public ClientController(ILogger<ClientController> logger, WebExConfigDefinition config)
        {
            Log = new FrameworkCallbackLogger(logger);
            Config = config;
        }

        [Route("[controller]/Config")]
        [HttpGet]
        public Task<ClientConfig> Get()
        {
            Log.Info("", "Config");

            var config = new ClientConfig();

            //sha1("3b6f88f2bed0f392" .. username, true);
            var user = Tools.RandomString.GetAlphanumLowercase(26);
            var computedPass = Crypto.SHA1Hex(Config.XmppUserPasswordSHA1Secret + user);

            config.xmpp.service = Config.XmppServiceUrl;
            config.xmpp.domain = Config.XmppDomain;
            config.xmpp.user = user;// "5qo9ek5q459bdch9qjmj1q4kb8";// "adobm56lv3hkch3n8ijmdukbf8";// Tools.RandomString.GetAlphanumLowercase(26);
            config.xmpp.pass = computedPass;// "1c60915db9b2d2a1b5aff96d709b135501cc992d";// Tools.RandomString.GetAlphanumLowercase(40);

            config.identity.identificatorUrlTemplate = Config.IdentificatorUrlTemplate;

            config.avatars.animationsUrlTemplate = Config.AnimationsUrlTemplate;
            config.avatars.animationsProxyUrlTemplate = Config.AnimationsProxyUrlTemplate;
            config.avatars.list = new List<string> {
                "0901/japan02_f",
                "0901/japan06_f",
                "0901/japan05_f",
                "0901/japan04_m",
                "0901/japan03_m",
                "0901/japan04_f",
                "0901/japan06_m",
                "0901/japan03_f",
                "0901/humanoidbot",
                "0901/japan05_m",
                "0901/hippoduck",
                "0901/japan02_m",
                "007/soccer_italy_male",
                "007/soccer_croatia_male",
                "007/soccer_russia_female",
                "007/soccer_netherlands_male",
                "007/soccer_spain_female",
                "007/soccer_turkey_male",
                "007/soccer_france_female",
                "007/soccer_portugese_male",
                "007/soccer_italy_female",
                "007/soccer_portugese_female",
                "007/soccer_poland_male",
                "007/soccer_czech_male",
                "007/soccer_romania_female",
                "007/soccer_greece_female",
                "007/soccer_russia_male",
                "007/soccer_swiss_female",
                "007/soccer_czech_female",
                "007/soccer_referee_male",
                "007/soccer_netherlands_female",
                "007/soccer_greece_male",
                "007/soccer_austria_male",
                "007/soccer_poland_female",
                "007/soccer_sweden_male",
                "007/soccer_sweden_female",
                "007/soccer_france_male",
                "007/soccer_croatia_female",
                "007/soccer_romania_male",
                "007/soccer_germany_female",
                "007/soccer_swiss_male",
                "007/soccer_germany_male",
                "007/soccer_austria_female",
                "007/soccer_spain_male",
                "007/soccer_turkey_female",
                "001/0003",
                "0811/japan01_f",
                "0811/japan01_m",
                "004/manga4",
                "004/girlietrousers",
                "004/alien",
                "004/sweatband",
                "004/jeansvest",
                "004/tennisw",
                "004/dino",
                "004/kitty1",
                "004/bill-tokyo",
                "004/egyptian2",
                "004/furvest",
                "004/wizard",
                "004/slimjeanshat",
                "004/astronaut",
                "004/beachchick",
                "004/beach-boy-reg",
                "004/beach-girl",
                "004/spock",
                "004/bavariangirl",
                "004/summerguy",
                "004/snake",
                "004/game1",
                "004/cheerleader",
                "004/comic2",
                "004/golfw",
                "004/troll",
                "004/bigwoman",
                "004/basketball",
                "004/spider",
                "004/fencer",
                "004/jacketngloves",
                "004/male7",
                "004/baseball",
                "004/sumo",
                "004/americanfootballplayer",
                "004/fantasy1",
                "004/indian",
                "004/female2",
                "004/skeleton",
                "004/elfe",
                "004/pinguin",
                "004/volleyball",
                "004/bavarianmale",
                "004/tennism",
                "004/napoleon",
                "004/puppy1",
                "004/fieldhockeyw",
                "004/shark",
                "004/rabbit",
                "004/rockstar",
                "004/horse",
                "004/martian",
                "004/dragon",
                "004/monkey",
                "004/dwarf",
                "004/tiger",
                "004/geisha",
                "004/hockey",
                "004/surfer",
                "004/female3",
                "004/cowboy",
                "004/male5",
                "004/weightlifter",
                "004/grimreaper",
                "004/comic1",
                "004/partydress",
                "004/panther",
                "004/knut",
                "004/funkypimp",
                "004/guitarplayer",
                "004/female4",
                "004/male2",
                "004/puppy2",
                "004/diving",
                "004/manga3",
                "004/comic3",
                "004/seal",
                "004/comic4",
                "004/polarbear",
                "004/fantasy4",
                "004/kitty2",
                "004/male3",
                "004/manga5",
                "004/female5",
                "004/charwoman",
                "004/kangaroo",
                "004/male1",
                "004/manga2",
                "004/fantasy3",
                "004/mermaid",
                "004/kathy",
                "004/beach-boy-xl",
                "004/fieldhockeym",
                "004/tom-tokyo",
                "004/dracula",
                "004/samurai",
                "004/slimjeansscarf",
                "004/lion",
                "004/girlieskirt",
                "004/karate",
                "004/rapper",
                "004/soccer",
                "004/game3",
                "004/skateboarding",
                "004/biker",
                "004/male4",
                "004/golfm",
                "004/female1",
                "004/game2",
                "004/egyptian",
                "004/fantasy5",
                "004/male6",
                "005/chimneysweeper",
                "005/santaclaus",
                "005/snowman",
                "005/reindeer",
                "002/sportive03_m",
                "002/business03_m",
                "002/child02_m",
                "002/sportive01_m",
                "002/business06_m",
                "002/casual04_f",
                "002/business01_f",
                "002/casual30_m",
                "002/sportive03_f",
                "002/casual16_m",
                "002/casual10_f",
                "002/business03_f",
                "002/casual03_m",
                "002/sportive07_m",
                "002/casual13_f",
                "002/casual09_m",
                "002/casual16_f",
                "002/child02_f",
                "002/sportive08_m",
                "002/casual15_m",
                "002/casual15_f",
                "002/casual01_f",
                "002/casual11_f",
                "002/sportive09_m",
                "002/casual20_f",
                "002/sportive02_f",
                "002/business05_m",
                "002/casual06_m",
                "002/casual10_m",
                "002/casual02_f",
                //"0810/lacatrina",
                //"0810/elcatrin",
                "0810/jester",
                //"0810/scarecrow",
                "0810/voodoodoll",
                "0810/monstertree",
                "0810/witch",
                "0810/zombie",
                //"0810/scarywoman",
                "003/festo_business03_m",
                "003/lancia_f1",
                "003/enbw_f_business",
                "003/newyorker_fishboneboy2",
                "003/singles2_kochm2",
                "003/newyorker_fishbonesister_girl1",
                "003/newyorker_fishboneboy1",
                "003/newyorker_smogboy1",
                "003/singles2_kochm1",
                "003/newyorker_fishbonesister_girl2",
                "003/newyorker_smogboy2",
                "003/enbw_m_business",
                "003/singles2_kochm3",
                "003/enbw_f_sport_04",
                "003/newyorker_amisu_girl2",
                "003/enbw_m_sport_04",
                "003/lancia_m1",
                //"003/intel_m",
                //"003/enbw_m_sport_02",
                "003/newyorker_amisu_girl1",
                //"003/intel_f",
                "006/Luxemburg",
                "006/Marx",
                "006/MartinLuther",
                "006/StatueOfLiberty",
                "006/Che",
                "006/Lennon",
                "006/Galileo",
                "006/MartinLutherKing",
                "006/Ghandi",
                };

            return Task.FromResult(config);
        }
    }
}
