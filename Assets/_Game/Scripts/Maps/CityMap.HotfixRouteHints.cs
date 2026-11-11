namespace LightBetweenCultures
{
    public sealed partial class CityMap
    {
        // UI only: never grants flags, changes route permissions or teleports Lumi.
        private string ExplainClosedRoute(int destination)
        {
            var id=game.Data.nodes[destination].id;
            if(game.Data.cityId=="gyeongju")
            {
                if(id=="G18"&&!game.World.Has("gyeongju.three_stars_placed"))
                    return "Cần đặt đủ 3 ký hiệu sao tại G4.";
                if(id=="G14"&&!game.World.Has("gyeongju.star_core_disabled"))
                    return "G14: tìm lối qua G8 hoặc xử lý lõi G18.";
                if(id=="G20"&&(!game.World.Has("gyeongju.beacon_lit")||!game.World.Has("hue.beacon_lit")||!game.World.Has("da_lat.beacon_lit")||!game.World.Has("jeonju.beacon_lit")))
                    return "Cần thắp đủ trạm Huế, Đà Lạt, Jeonju, Gyeongju.";
                if(id=="G21"&&!game.World.Has("gyeongju.secret_herb_found"))
                    return "Hãy nói chuyện với Min ở G3 để mở lối dược liệu.";
            }
            return "Chưa có đường hợp lệ. Xem Nhật ký và các mốc gần.";
        }
    }
}
