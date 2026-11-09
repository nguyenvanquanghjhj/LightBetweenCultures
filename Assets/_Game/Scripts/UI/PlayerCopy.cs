using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
namespace LightBetweenCultures
{
    // Presentation only. Stable IDs, save keys and event dispatch never pass here.
    public static class PlayerCopy
    {
        static readonly Regex ids=new Regex(@"\b(?:[HUDJGS]\d{1,2}|[HUDJGS]Q\d{2}|relic_[a-z0-9_]+|fragment_[a-z0-9_]+)\b");
        static ContentDatabase cachedContent;
        static readonly Dictionary<string,string> displayNames=new Dictionary<string,string>();
        static void Names(GameBootstrap game)
        {
            if(ReferenceEquals(cachedContent,game.Content))return;
            cachedContent=game.Content;displayNames.Clear();
            foreach(var city in game.Content.cities)foreach(var node in city.nodes)displayNames[node.id]=node.name;
            foreach(var quest in game.Content.quests)displayNames[quest.id]=quest.title;
            if(game.Relics!=null)
            {foreach(var relic in game.Relics.Catalog.relics)displayNames[relic.id]=relic.name;foreach(var fragment in game.Relics.Catalog.fragments)displayNames[fragment.id]=fragment.name;}
        }
        public static string Display(string value)
        {
            if(string.IsNullOrEmpty(value))return value??"";
            var game=GameBootstrap.Instance;
            if(game==null||game.Content==null)return value;
            Names(game);
            return ids.Replace(value,m=>displayNames.TryGetValue(m.Value,out var name)?name:m.Value)
                .Replace("World Map","bản đồ thế giới").Replace("WorldMap","bản đồ thế giới")
                .Replace("Làn sương trang trí giãn ra; các lối đi và vùng chưa khám phá vẫn theo tiến độ cũ.","Làn sương dần giãn ra. Những dấu sáng nhắc Lumi về hành trình qua đồi thông.");
        }
        public static string ActionLabel(string id,string fallback,EventDefinition definition)
        {
            if(id.StartsWith("relic:",StringComparison.Ordinal))return "Ghi lại mảnh · "+(GameBootstrap.Instance.Relics.Fragment(id.Substring(6))?.name??"manh mối");
            if(id.StartsWith("relic_restore",StringComparison.Ordinal))
            {
                var g=GameBootstrap.Instance;var r=id=="relic_restore"?g.Relics.Definition("relic_hoi_an_lantern"):g.Relics.Definition(id.Substring("relic_restore:".Length));
                if(r!=null&&!g.Relics.Restored(r.id)&&g.Relics.Count(r)==4)return g.Relics.Assembled(r.id)?"Thử thách phục dựng · "+r.trial:"Ghép bốn mảnh · "+r.name;
            }
            switch(id)
            {
                case "scene":return GameBootstrap.Instance?.Data.cityId=="hoi_an"?(GameBootstrap.Instance.World.Has("hoi_an.lamp_eye_found")?"Khám phá lại khu hẻm":"Khám phá hẻm · Tìm Mắt Đèn"):"Khám phá địa điểm";
                case "tutorial_return":return "Trao Mắt Đèn cho Cô Mai";
                case "restore":return "Đồng bộ trạm ánh sáng";
                case "checkin":return "Chụp ảnh · Lưu vào nhật ký";
                case "checkin_view":return "Xem ảnh đã lưu";
                case "collect":return (definition?.title??"").IndexOf("rương",StringComparison.OrdinalIgnoreCase)>=0?"Mở rương":"Nhận vật phẩm";
                case "portal":return "Xem thành phố trên bản đồ thế giới";
                case "complete":return (GameBootstrap.Instance?.Content.Quest(definition?.questId)?.requiredItems?.Length??0)>0?"Giao vật phẩm nhiệm vụ":"Báo tin hoàn thành";
                case "talk":return definition?.type=="npc"?string.IsNullOrEmpty(definition.questId)?"Trò chuyện":"Hỏi việc cần làm":"Quan sát địa điểm";
                default:return Display(fallback);
            }
        }
        public static string DialogueText(string value,EventDefinition definition)
        {
            // Display copy only; keep the authored event, proofs and dispatch intact.
            if(definition?.id=="restore_lantern")
                value=(value??"")
                    .Replace("Trạm giao lưu đã đồng bộ; Huế và Jeonju đáp lại tín hiệu. Chiếc đèn đầu tiên trước tiệm vẫn sáng. Đèn Mắt Phố gìn giữ câu chuyện riêng của cả phố; cháu có thể tiếp tục hành trình hoặc ở lại giúp bà con.","Huế và Jeonju đã mở. Chiếc đèn trước tiệm vẫn sáng; cháu có thể lên đường hoặc ở lại giúp bà con.")
                    .Replace("Trạm giao lưu vẫn giữ tín hiệu Huế và Jeonju; ","")
                    .Replace("Trạm đã đồng bộ và hai tuyến Huế, Jeonju đã mở. ","")
                    .Replace("Việc lắp linh kiện sẽ đồng bộ trạm giao lưu. Đèn Mắt Phố là nghi thức riêng sau đó, không phải điều kiện để mở các thành phố.","Đèn Mắt Phố là nghi thức riêng, không phải điều kiện mở các thành phố.")
                    .Replace("Chìa Kết Nối đã được dùng cùng Mắt Đèn và Lõi Giấy để đồng bộ trạm H7. Huế và Jeonju mở theo tín hiệu trạm; không cần mang chìa sang thành phố khác.","Mắt Đèn, Lõi Giấy và Chìa Kết Nối đã đồng bộ trạm; không cần mang chìa sang thành phố khác.");
            return Display(value);
        }
        public static AdventureSymbol ActionIcon(string id)
        {
            if(id.StartsWith("checkin")||id=="photo_h6")return AdventureSymbol.Camera;
            if(id=="portal")return AdventureSymbol.Globe;
            if(id=="battle")return AdventureSymbol.BossAlert;
            if(id=="accept"||id.StartsWith("accept:"))return AdventureSymbol.Journal;
            if(id=="rest")return AdventureSymbol.Plus;
            if(id=="puzzle")return AdventureSymbol.Puzzle;
            if(id.StartsWith("collect")||id=="tutorial_return"||id=="restore"||id.StartsWith("complete")||id.StartsWith("relic"))return AdventureSymbol.Check;
            if(id=="scene"||id.StartsWith("leisure"))return AdventureSymbol.Arrow;
            return AdventureSymbol.Chat;
        }
        public static string Unavailable(GameBootstrap game,int node,EventOption option,EventDefinition definition)
        {
            if(option.id.StartsWith("relic_restore"))
            {var r=option.id=="relic_restore"?game.Relics.Definition("relic_hoi_an_lantern"):game.Relics.Definition(option.id.Substring("relic_restore:".Length));return r==null?"Tìm đủ bốn mảnh trước.":RelicGuide.Next(game,r);}
            if(option.id=="complete"||option.id=="restore"||option.id.StartsWith("complete:",StringComparison.Ordinal))
            {
                var questId=definition?.questId;
                if(option.id.StartsWith("complete:",StringComparison.Ordinal))
                    questId=game.Content.events.FirstOrDefault(e=>e.cityId==game.Data.cityId&&e.nodeId==game.Data.nodes[node].id&&e.id==option.id.Substring(9))?.questId;
                var quest=game.Content.Quest(questId);
                if(quest!=null)
                {
                    var missing=quest.requiredItems.Select(i=>new{item=i,needed=i.amount+(quest.reserveItems??Array.Empty<ItemAmount>()).Where(r=>r.itemId==i.itemId).Select(r=>r.amount).DefaultIfEmpty(0).Max()-game.Inventory.Count(i.itemId)})
                        .Where(i=>i.needed>0).Select(i=>(game.Content.Item(i.item.itemId)?.name??"vật phẩm nhiệm vụ")+" × "+i.needed).ToArray();
                    if(missing.Length>0)return "Còn thiếu: "+string.Join(", ",missing)+".";
                    if(!game.Quests.IsActive(quest.id))return "Hãy nhận nhiệm vụ trước.";
                }
                return "Hãy xem việc cần làm trong nhật ký nhiệm vụ.";
            }
            return "Cần hoàn thành việc đang chờ tại địa điểm này. Xem lời nhắc bên trên.";
        }
        public static bool AutoGuide(GameBootstrap game)
        {
            var args=Environment.GetCommandLineArgs();
            if(args.Contains("--gameplay-smoke")&&!args.Contains("--player-experience-after"))return false;
            return game.Data.cityId=="hoi_an"&&game.Save.accepted.Count==0&&game.Save.visited.Count<=1&&PlayerPrefs.GetInt(GuideKey,0)==0;
        }
        public static string GuideKey=>Environment.GetCommandLineArgs().Contains("--gameplay-smoke")?"LBC.PlayerGuide.QA":"LBC.PlayerGuide.Seen.v1";
        public static string Requirement(string flag)
        {
            var game=GameBootstrap.Instance;if(game==null)return "Tiếp tục nhiệm vụ trong nhật ký.";
            var quest=game.Content.quests.FirstOrDefault(q=>q.completionFlag==flag||(q.flags??Array.Empty<string>()).Contains(flag));
            if(quest!=null)return "Hoàn thành nhiệm vụ: "+quest.title;
            var puzzle=game.Content.puzzles.FirstOrDefault(p=>p.completionFlag==flag||flag=="puzzle:"+p.id||(p.flags??Array.Empty<string>()).Contains(flag));
            if(puzzle!=null)return "Hoàn thành thử thách: "+puzzle.title;
            var source=game.Content.events.FirstOrDefault(e=>e.completionFlag==flag||(e.flags??Array.Empty<string>()).Contains(flag)||(e.onTalkFlags??Array.Empty<string>()).Contains(flag));
            if(source!=null)
            {
                var node=game.Content.City(source.cityId)?.nodes.FirstOrDefault(n=>n.id==source.nodeId);
                return "Hoàn thành việc tại "+(node?.name??source.title);
            }
            return "Tiếp tục việc đang được nhắc trong nhật ký nhiệm vụ.";
        }
        public static string CityIdentity(string city)
        {
            switch(city)
            {
                case "hoi_an":return "Phố cổ, đèn lồng và những người giữ nghề. Mỗi ánh đèn được nối lại mang theo một câu chuyện.";
                case "hue":return "Dọc dòng sông, dấu gốm và văn khắc lưu giữ nhịp ký ức của vùng đất di sản.";
                case "da_lat":return "Theo dấu gió giữa hoa và sương. Những tín hiệu nhỏ dẫn Lumi qua miền đồi thông.";
                case "jeonju":return "Trong những mái hanok, giấy hanji và chén trà nối câu chuyện của người làm nghề.";
                case "gyeongju":return "Di tích, tượng đá và tiếng chuông giữ những lời nhắn từ quá khứ. Cách Lumi giúp đỡ cũng để lại dấu ấn.";
                case "seoul":return "Ánh sáng hiện đại gặp ký ức truyền thống. Những câu chuyện đã thu nhặt cùng hướng về Lễ Kết Nối.";
                default:return "Cùng Lumi khám phá những câu chuyện văn hóa.";
            }
        }
        public static string ItemPurpose(GameBootstrap game,string id)
        {
            switch(id)
            {
                case "ancient_seal":return "Đối chiếu tại bệ phong ấn Gyeongju và relay di sản Seoul. Giữ lại ấn; không tiêu hao.";
                case "lamp_eye":return !game.Quests.Completed?"Mang Mắt Đèn về Tiệm đèn Cô Mai để trao trả.":"Dùng cùng Lõi Giấy và Chìa Kết Nối để đồng bộ Trạm đèn hội quán.";
                case "link_key":return "Dùng cùng Mắt Đèn và Lõi Giấy tại Trạm đèn hội quán. Chỉ nhận chìa chưa mở thành phố.";
                case "paper_core":return "Mang Lõi Giấy cùng Mắt Đèn và Chìa Kết Nối tới Trạm đèn hội quán.";
            }
            var quest=game.Content.quests.FirstOrDefault(q=>game.Quests.IsActive(q.id)&&!game.Quests.IsCompleted(q.id)&&(q.requiredItems??Array.Empty<ItemAmount>()).Any(i=>i.itemId==id));
            if(quest==null)return "";
            var city=game.Content.City(string.IsNullOrEmpty(quest.completeCityId)?quest.cityId:quest.completeCityId);
            var place=city?.nodes.FirstOrDefault(n=>n.id==quest.completeAtNode)?.name;
            return string.IsNullOrEmpty(place)?"Dùng cho nhiệm vụ "+quest.title+". Xem nhật ký để biết nơi giao.":"Mang tới "+place+" cho nhiệm vụ "+quest.title+".";
        }
        public static string Journal(GameBootstrap game)
        {
            var city=game.Content.City(game.Data.cityId);var here=game.Data.nodes[game.Save.node].name;
            var text="LẦN DỪNG CHÂN NÀY\n"+city.name+" · "+here+"\n"+CityIdentity(city.id)+"\n\nVIỆC TIẾP THEO\n"+game.Quests.Guidance()+"\n\nNHIỆM VỤ ĐÃ NHẬN\n"+JournalTasks(game);
            text+="\n\nSỔ DI VẬT\n";
            foreach(var relic in game.Relics.Catalog.relics)
                text+=relic.name+" · "+RelicGuide.Next(game,relic)+"\n";
            text+="Di vật không dùng để mở thành phố hoặc vào boss. Lễ Kết Nối cuối cần boss được giải quyết và sáu di vật phục dựng.";
            var visited=game.Save.visited.Where(i=>i>=0&&i<game.Data.nodes.Length).Select(i=>game.Data.nodes[i].name);
            text+="\n\nDẤU CHÂN TẠI "+city.name.ToUpperInvariant()+"\n"+string.Join(" · ",visited);
            var photos=game.Content.checkIns.Where(c=>game.World.Has(c.completionFlag)).ToArray();
            text+="\n\nẢNH ĐÃ LƯU\n"+(photos.Length==0?"Chưa lưu ảnh. Chỉ khi xác nhận ghi dấu, ảnh mới vào nhật ký.":string.Join(" · ",photos.Select(p=>p.title)));
            var rests=LeisurePresentation.Load().entries.Where(e=>e.cityId==city.id&&e.enabled&&game.Save.visited.Any(i=>i>=0&&i<game.Data.nodes.Length&&game.Data.nodes[i].id==e.nodeId)).ToArray();
            text+="\n\nNHỮNG KHOẢNG NGHỈ\n"+(rests.Length==0?"Khám phá các điểm đã mở để tìm một khoảng nghỉ cùng Lumi.":string.Join(" · ",rests.Select(e=>e.title)))+"\nĐây là trải nghiệm tùy chọn, có thể chơi lại. Không cần hoàn thành chúng để mở đường hoặc nhận di vật.";
            return Display(text);
        }
        static string JournalTasks(GameBootstrap game)
        {
            // Presentation only: keep the original quest progress and IDs. Old
            // thank-you paragraphs must not push the current task off the page.
            var accepted=game.Save.accepted.Select(game.Content.Quest).Where(q=>q!=null).ToArray();
            var city=game.Data.cityId;
            var active=accepted.Where(q=>!game.Quests.IsCompleted(q.id))
                .OrderBy(q=>q.cityId==city||q.completeCityId==city||q.giverCityId==city?0:1)
                .ThenBy(q=>game.Quests.CanComplete(q.id)?0:1).ToArray();
            var text=active.Length==0?"Không còn nhiệm vụ đang làm trong danh sách đã nhận.":string.Join("\n\n",active.Select(q=>"• "+q.title+"\n"+game.Quests.Progress(q.id,q.id!="HQ01"||!game.Quests.Completed)));
            var completed=accepted.Where(q=>game.Quests.IsCompleted(q.id)).ToArray();
            if(completed.Length>0)text+="\n\nĐÃ HOÀN THÀNH · "+completed.Length+"\n"+string.Join(" · ",completed.Select(q=>"✓ "+q.title));
            return text;
        }
        public static string LeisureHelp(string mechanic)
        {
            string action;
            switch(mechanic)
            {
                case "swing":action="Bắt đầu để Lumi cùng ghế xích đu đung đưa dưới giàn đèn.";break;
                case "boat":action="Bắt đầu để Lumi đạp thuyền trên một đoạn sông ngắn.";break;
                case "release":action="Kéo bông hoa đăng ra vùng mặt sông bên phải Lumi rồi thả tay. Hướng kéo sẽ quyết định hướng hoa trôi. Có thể chạm hoa hoặc chọn Thả hoa đăng để dùng hướng sẵn có.";break;
                case "seat":action="Bắt đầu để Lumi trải nghiệm góc ngai vàng trong không gian di sản.";break;
                case "pick":action="Bắt đầu để Lumi hái những quả dâu chín trong vườn.";break;
                case "fly":action="Kéo cánh diều trên bầu trời để đổi hướng bay. Dây diều vẫn nối với Lumi; không cần giữ đúng một điểm hay đạt điểm số.";break;
                case "water":action="Bắt đầu để Lumi tưới những chậu cây trong sân hanok.";break;
                case "tea":action="Bắt đầu để Lumi nâng chén trà và nghỉ chân bên bàn.";break;
                case "bell":action="Kéo chày chuông rồi thả. Độ dài nét kéo chọn nhịp đung đưa nhẹ hoặc rộng hơn; không có yêu cầu canh giờ hay phần thưởng.";break;
                case "wish":action="Kéo thẻ ước từ Lumi đến ô sáng trên giá rồi thả tay. Thả lệch sẽ trả thẻ lại để bạn thử tiếp, không mất vật phẩm.";break;
                case "snack":action="Bắt đầu để Lumi thưởng thức món ăn tại Seoul.";break;
                case "dance":action="Bắt đầu để Lumi hòa theo nhịp nhạc ở Seoul.";break;
                default:action="Bắt đầu để trải nghiệm hoạt cảnh văn hóa cùng Lumi.";break;
            }
            return action+"\n\nThanh phía dưới cho biết thời gian của hoạt cảnh thật. Bạn có thể trở về bản đồ bất cứ lúc nào hoặc Chơi lại sau khi kết thúc.\n\nĐây là khoảng nghỉ, không phải thử thách có thắng/thua; không trao vật phẩm, tiền hoặc tiến độ nhiệm vụ.";
        }
    }
}
