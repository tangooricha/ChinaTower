using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ChinaTower.StationPlanning.Models;
using ChinaTower.StationPlanning.Algorithms;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class WgsDisExt
    {
        public static IServiceCollection AddWgsDis(this IServiceCollection self)
        {
            return self.AddScoped<WgsDis>();
        }
    }
}

namespace ChinaTower.StationPlanning.Algorithms
{
    public class WgsDis
    {
        private IConfiguration Config { get; set; }
        public WgsDis(IConfiguration config)
        {
            Config = config;
        }
        public IList<KeyValuePair<Tower, Tower>> Solve(IList<Tower> towers)
        {
            var ret = new List<KeyValuePair<Tower, Tower>>();
            for (var i = 0; i < towers.Count; i++)
            {
                for (var j = i + 1; j < towers.Count; j++)
                {
                    var dis = Suggest.GetDistance(towers[i].Lat, towers[i].Lon, towers[j].Lat, towers[j].Lon);
                    var flag = false;
                    if (towers[i].Status == TowerStatus.预选 || towers[j].Status == TowerStatus.预选)
                    {
                        switch (towers[j].Scene)
                        {
                            case TowerScene.郊区:
                                if (dis < Convert.ToDouble(Config["Settings:Plan:Suburb"]))
                                    flag = true;
                                break;
                            case TowerScene.密集城区:
                                if (dis < Convert.ToDouble(Config["Settings:Plan:Crowded"]))
                                    flag = true;
                                break;
                            case TowerScene.农村:
                                if (dis < Convert.ToDouble(Config["Settings:Plan:Village"]))
                                    flag = true;
                                break;
                            default:
                                if (dis < Convert.ToDouble(Config["Settings:Plan:City"]))
                                    flag = true;
                                break;
                        }
                    }
                    else
                    {
                        switch (towers[j].Scene)
                        {
                            case TowerScene.郊区:
                                if (dis < Convert.ToDouble(Config["Settings:Share:Suburb"]))
                                    flag = true;
                                break;
                            case TowerScene.密集城区:
                                if (dis < Convert.ToDouble(Config["Settings:Share:Crowded"]))
                                    flag = true;
                                break;
                            case TowerScene.农村:
                                if (dis < Convert.ToDouble(Config["Settings:Share:Village"]))
                                    flag = true;
                                break;
                            default:
                                if (dis < Convert.ToDouble(Config["Settings:Share:City"]))
                                    flag = true;
                                break;
                        }
                    }
                    if (flag)
                    {
                        ret.Add(new KeyValuePair<Tower, Tower>(towers[i], towers[j]));
                    }
                }
            }
            return ret;
        }
    }
}
