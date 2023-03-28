using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using TravelMaker.Models;
using TravelMaker.Security;

namespace TravelMaker.Controllers
{
    /// <summary>
    ///     行程相關
    /// </summary>
    [RoutePrefix("api/tour")]
    public class TourController : ApiController
    {
        private TravelMakerDbContext _db = new TravelMakerDbContext();

        /// <summary>
        ///     隨機產生行程(含鎖點擴充)
        /// </summary>
        [HttpPost]
        [Route("make")]
        public IHttpActionResult Make(GetJourneysView choose)
        {
            List<int> types = new List<int>();

            if (choose.CategoryId.Contains(0)) //不限類型，隨機挑三個類別
            {
                types = _db.Categories.Where(a => a.CategoryId!=8|| a.CategoryId != 9).Select(a=>a.CategoryId).Take(3).ToList();
            }
            else
            {
                types = choose.CategoryId.ToList();
            }


            //類別加白天餐廳8or晚上餐廳9
            if (!types.Contains(4))
            {
                types.Add(8);
            }
            else if (types.Count > 1)
            {
                types.Add(8);
                types.Add(9);
            }
            else
            {
                types.Add(9);
            }


            //景點距離長短設定
            int shortDistance, longDistance;
            if (choose.Transports == "走路")
            {
                shortDistance = 500;
                longDistance = 1500;
            }
            else
            {
                shortDistance = 500;
                longDistance = 6000;
            }



            List<AttractionList> attractions = new List<AttractionList>();

            //先撈符合類型、地區的景點id並隨機排序
            if (choose.Elong == 0.0 && choose.Nlat == 0.0)//選地區
            {
                //地區清單
                List<int> districts = new List<int>();

                if (choose.DistrictName.Contains("不限")) //隨機選4區
                {
                    districts = _db.Districts.Select(a => a.DistrictId).OrderBy(x => Guid.NewGuid()).Take(4).ToList();
                }
                else //指定地區
                {
                    foreach (string district in choose.DistrictName)
                    {
                        int districtId = _db.Districts.Where(d => d.DistrictName == district).Select(d => d.DistrictId).FirstOrDefault();
                        districts.Add(districtId);
                    }
                }

                attractions = _db.CategoryAttractions.Where(a => a.Attraction.OpenStatus == true
                                                                 && types.Contains(a.CategoryId)
                                                                 && districts.Contains(a.Attraction.DistrictId))
                                                     .Select(a => new AttractionList
                                                     {
                                                         AttractionId = a.Attraction.AttractionId,
                                                         IsRestaurant = a.CategoryId == 8 || a.CategoryId == 9 ? 1 : 0
                                                     })
                                                     .Distinct().OrderBy(x => Guid.NewGuid()).ToList();
            }
            else//開啟定位
            {
                decimal elong = Convert.ToDecimal(choose.Elong);
                decimal nlat = Convert.ToDecimal(choose.Nlat);
                decimal elongAdd, nlatAdd;
                if (choose.Transports == "走路")//1.7km
                {
                    elongAdd = Convert.ToDecimal(0.0098);
                    nlatAdd = Convert.ToDecimal(0.0076);
                }
                else//3km
                {
                    elongAdd = Convert.ToDecimal(0.0127);
                    nlatAdd = Convert.ToDecimal(0.0135);
                }


                attractions = _db.CategoryAttractions.Where(a => a.Attraction.OpenStatus == true
                                                                && types.Contains(a.CategoryId)
                                                                && elong - elongAdd < a.Attraction.Elong
                                                                && a.Attraction.Elong < elong + elongAdd
                                                                && nlat - nlatAdd < a.Attraction.Nlat
                                                                && a.Attraction.Nlat < nlat + nlatAdd)
                                                     .Select(a => new AttractionList
                                                     {
                                                         AttractionId = a.Attraction.AttractionId,
                                                         IsRestaurant = a.CategoryId == 8 || a.CategoryId == 9 ? 1 : 0
                                                     })
                                                     .Distinct().OrderBy(x => Guid.NewGuid()).ToList();
            }



            //把距離加進字典(因為Distinct()跟DbGeography衝突，所以分開存)
            Dictionary<int, DbGeography> locationCache = new Dictionary<int, DbGeography>();
            foreach (var attraction in attractions)
            {
                int attractionId = attraction.AttractionId;

                locationCache[attractionId] = _db.Attractions
                                             .Where(a => a.AttractionId == attractionId)
                                             .Select(a => a.Location)
                                             .FirstOrDefault();
            }


            //返回景點id之List
            List<int> returnAttractionId = new List<int>();

            foreach (int id in choose.AttractionId)
            {
                if (id != 0)
                {
                    returnAttractionId.Add(id);
                }
            }


            //計算真的要撈的景點數
            int attractionCount = choose.AttrCounts;
            int realAttractionCount = attractionCount - returnAttractionId.Count;


            //計算返回id裡幾間餐廳了
            int restaurantCount = 0;

            foreach (int id in returnAttractionId)
            {
                if (id != 0)
                {
                    int category = _db.CategoryAttractions.Where(a => a.AttractionId == id)
                                     .Select(a => a.CategoryId).FirstOrDefault();

                    if (category == 8 || category == 9)
                    {
                        restaurantCount++;
                    }
                }
            }



            //暫存剩餘撈的點
            List<int> tempId = new List<int>();

            if (realAttractionCount % 2 == 0)   //剩餘撈取景點是偶數
            {
                for (int i = 0; i < attractions.Count; i++)
                {
                    //景點變數初始值
                    int isrestaurantA = 0, isrestaurantB = 0, isrestaurantC = 0, isrestaurantD = 0, isrestaurantE = 0, isrestaurantF = 0, isrestaurantG = 0, isrestaurantH = 0;
                    DbGeography LocationA = null; DbGeography LocationB = null; DbGeography LocationC = null; DbGeography LocationD = null; DbGeography LocationE = null; DbGeography LocationF = null; DbGeography LocationG = null; DbGeography LocationH = null;
                    bool okB = true, okD = true, okF = true, okH = true;
                    //迴圈兩次以上撈不到點的話距離加長
                    if (i > 1)
                    {
                        shortDistance += 50;
                        longDistance += 50;
                    }


                    //1.景點A(AB近)   
                    var attractionA = attractions[i];
                    int attractionAId = attractionA.AttractionId;
                    isrestaurantA = attractionA.IsRestaurant;

                    //必為景點
                    if (!returnAttractionId.Contains(attractionAId) && isrestaurantA == 0)
                    {
                        LocationA = locationCache[attractionAId];
                        tempId.Add(attractionAId);
                    }
                    else
                    {
                        continue;
                    }




                    //2.景點B(AB近)
                    var attractionTemp = locationCache
                                            .Where(a => a.Value.Distance(LocationA) < shortDistance
                                                         && !tempId.Contains(a.Key)
                                                         && !returnAttractionId.Contains(a.Key))
                                            .Select(a => a.Key);

                    foreach (int id in attractionTemp)
                    {
                        isrestaurantB = attractions.Where(a => a.AttractionId == id)
                                                   .Select(a => a.IsRestaurant).FirstOrDefault();

                        //可以是景點或餐廳，但總餐廳數不能超過一半
                        if (isrestaurantB == 0 || isrestaurantA + restaurantCount < attractionCount / 2)
                        {
                            LocationB = locationCache[id];
                            tempId.Add(id);
                            okB = false;
                            break;
                        }
                    }

                    if (okB)
                    {
                        tempId.Clear();
                        continue;
                    }
                    else if (realAttractionCount == 2)
                    {
                        break;
                    }




                    //3.景點C(CD近)
                    attractionTemp = locationCache
                                     .Where(a => shortDistance < a.Value.Distance(LocationB)
                                                && a.Value.Distance(LocationB) < longDistance
                                                && !tempId.Contains(a.Key)
                                                && !returnAttractionId.Contains(a.Key))
                                     .Select(a => a.Key);

                    foreach (int id in attractionTemp)
                    {
                        isrestaurantC = attractions.Where(a => a.AttractionId == id)
                                                   .Select(a => a.IsRestaurant).FirstOrDefault();
                        LocationC = locationCache[id];


                        //總餐廳數不超過一半
                        if (isrestaurantC == 0
                            || isrestaurantA + isrestaurantB + restaurantCount < attractionCount / 2)
                        {
                            //4.景點D
                            var attractionTemp2 = locationCache
                                            .Where(a => a.Value.Distance(LocationC) < shortDistance
                                                         && id != a.Key
                                                         && !tempId.Contains(a.Key)
                                                         && !returnAttractionId.Contains(a.Key))
                                            .Select(a => a.Key);


                            foreach (int id2 in attractionTemp2)
                            {
                                isrestaurantD = attractions.Where(a => a.AttractionId == id2)
                                                           .Select(a => a.IsRestaurant).FirstOrDefault();

                                //C或D是餐廳，如果總餐廳已超過一半就只能是景點
                                if (isrestaurantD == 0 || (isrestaurantC == 0 && isrestaurantA + isrestaurantB + restaurantCount < attractionCount / 2))
                                {
                                    LocationD = locationCache[id2];
                                    tempId.Add(id);
                                    tempId.Add(id2);
                                    okD = false;
                                    break;
                                }
                            }
                        }
                        if (okD == false)
                        {
                            break;
                        }
                    }

                    if (okD)
                    {
                        tempId.Clear();
                        continue;
                    }
                    else if (realAttractionCount == 4)
                    {
                        break;
                    }





                    //5.景點E(EF近)
                    attractionTemp = locationCache
                                     .Where(a => shortDistance < a.Value.Distance(LocationD)
                                                && a.Value.Distance(LocationD) < longDistance
                                                && !tempId.Contains(a.Key)
                                                && !returnAttractionId.Contains(a.Key))
                                     .Select(a => a.Key);

                    foreach (int id in attractionTemp)
                    {
                        isrestaurantE = attractions.Where(a => a.AttractionId == id)
                                                   .Select(a => a.IsRestaurant).FirstOrDefault();
                        LocationE = locationCache[id];


                        //總餐廳數不超過一半
                        if (isrestaurantE == 0 || isrestaurantA + isrestaurantB + isrestaurantC + isrestaurantD + restaurantCount < attractionCount / 2)
                        {
                            //6.景點F
                            var attractionTemp2 = locationCache
                                            .Where(a => a.Value.Distance(LocationE) < shortDistance
                                                         && id != a.Key
                                                         && !tempId.Contains(a.Key)
                                                         && !returnAttractionId.Contains(a.Key))
                                            .Select(a => a.Key);


                            foreach (int id2 in attractionTemp2)
                            {
                                isrestaurantF = attractions.Where(a => a.AttractionId == id2)
                                                           .Select(a => a.IsRestaurant).FirstOrDefault();

                                //E或F是餐廳，如果總餐廳已超過一半就只能是景點
                                if (isrestaurantF == 0 || (isrestaurantE == 0 && isrestaurantA + isrestaurantB + isrestaurantC + isrestaurantD + restaurantCount < attractionCount / 2))
                                {
                                    LocationF = locationCache[id2];
                                    tempId.Add(id);
                                    tempId.Add(id2);
                                    okF = false;
                                    break;
                                }
                            }
                        }
                        if (okF == false)
                        {
                            break;
                        }
                    }

                    if (okF)
                    {
                        tempId.Clear();
                        continue;
                    }
                    else if (realAttractionCount == 6)
                    {
                        break;
                    }





                    //7.景點G(GH近)
                    attractionTemp = locationCache
                                     .Where(a => shortDistance < a.Value.Distance(LocationF)
                                                && a.Value.Distance(LocationF) < longDistance
                                                && !tempId.Contains(a.Key)
                                                && !returnAttractionId.Contains(a.Key))
                                     .Select(a => a.Key);

                    foreach (int id in attractionTemp)
                    {
                        isrestaurantG = attractions.Where(a => a.AttractionId == id)
                                                   .Select(a => a.IsRestaurant).FirstOrDefault();
                        LocationG = locationCache[id];



                        //8.景點H
                        var attractionTemp2 = locationCache
                                        .Where(a => a.Value.Distance(LocationG) < shortDistance
                                                     && id != a.Key
                                                     && !tempId.Contains(a.Key)
                                                     && !returnAttractionId.Contains(a.Key))
                                        .Select(a => a.Key);


                        foreach (int id2 in attractionTemp2)
                        {
                            isrestaurantH = attractions.Where(a => a.AttractionId == id2)
                                                       .Select(a => a.IsRestaurant).FirstOrDefault();

                            //G或H是餐廳
                            if (isrestaurantG + isrestaurantH < 2)
                            {
                                LocationH = locationCache[id2];
                                tempId.Add(id);
                                tempId.Add(id2);
                                okH = false;
                                break;
                            }
                        }
                        if (okH == false)
                        {
                            break;
                        }
                    }

                    if (okH)
                    {
                        tempId.Clear();
                        continue;
                    }
                    else if (realAttractionCount == 8)
                    {
                        break;
                    }
                }

            }
            else  //剩餘撈取景點是奇數(1,3,5=2+3,7=2+2+3)
            {
                for (int i = 0; i < attractions.Count; i++)
                {
                    //景點變數初始值
                    int isrestaurantA = 0, isrestaurantB = 0, isrestaurantC = 0, isrestaurantD = 0, isrestaurantE = 0, isrestaurantF = 0, isrestaurantG = 0;
                    DbGeography LocationA = null; DbGeography LocationB = null; DbGeography LocationC = null; DbGeography LocationD = null; DbGeography LocationE = null; DbGeography LocationF = null; DbGeography LocationG = null;
                    bool okA = true, okC = true, okE = true, okG = true;
                    //迴圈兩次以上撈不到點的話距離加長
                    if (i > 1)
                    {
                        shortDistance += 50;
                        longDistance += 50;
                    }


                    //1.景點A(ABC近)
                    var attractionA = attractions[i];
                    int attractionAId = attractionA.AttractionId;
                    isrestaurantA = attractionA.IsRestaurant;

                    //必為景點
                    if (!returnAttractionId.Contains(attractionAId) && isrestaurantA == 0)
                    {
                        LocationA = locationCache[attractionAId];
                        tempId.Add(attractionAId);
                        okA = false;
                    }
                    else
                    {
                        continue;
                    }

                    if (okA == false && realAttractionCount == 1)
                    {
                        break;
                    }



                    //2.景點B(ABC近)
                    var attractionTemp = locationCache.Where(a => a.Value.Distance(LocationA) < shortDistance
                                                                && !tempId.Contains(a.Key)
                                                                && !returnAttractionId.Contains(a.Key))
                                                      .Select(a => a.Key);

                    foreach (int id in attractionTemp)
                    {
                        isrestaurantB = attractions.Where(a => a.AttractionId == id)
                                                   .Select(a => a.IsRestaurant).FirstOrDefault();
                        LocationB = locationCache[id];


                        //總餐廳數不超過一半
                        if (isrestaurantB == 0 || restaurantCount < attractionCount / 2)
                        {
                            //3.景點C
                            var attractionTemp2 = locationCache
                                                            .Where(a => a.Value.Distance(LocationB) < shortDistance
                                                                 && id != a.Key
                                                                 && !tempId.Contains(a.Key)
                                                                 && !returnAttractionId.Contains(a.Key))
                                                            .Select(a => a.Key);


                            foreach (int id2 in attractionTemp2)
                            {
                                isrestaurantC = attractions.Where(a => a.AttractionId == id2)
                                                           .Select(a => a.IsRestaurant).FirstOrDefault();

                                //B或C是餐廳，如果總餐廳已超過一半就只能是景點
                                if (isrestaurantC == 0 ||
                                    (isrestaurantB == 0 && restaurantCount < attractionCount / 2))
                                {
                                    LocationC = locationCache[id2];
                                    tempId.Add(id);
                                    tempId.Add(id2);
                                    okC = false;
                                    break;
                                }
                            }
                        }
                        if (okC == false)
                        {
                            break;
                        }
                    }

                    if (okC)
                    {
                        tempId.Clear();
                        continue;
                    }
                    else if (realAttractionCount == 3)
                    {
                        break;
                    }





                    //4.景點D(DE近)
                    attractionTemp = locationCache
                                     .Where(a => shortDistance < a.Value.Distance(LocationC)
                                                && a.Value.Distance(LocationC) < longDistance
                                                && !tempId.Contains(a.Key)
                                                && !returnAttractionId.Contains(a.Key))
                                     .Select(a => a.Key);

                    foreach (int id in attractionTemp)
                    {
                        isrestaurantD = attractions.Where(a => a.AttractionId == id)
                                                   .Select(a => a.IsRestaurant).FirstOrDefault();
                        LocationD = locationCache[id];


                        //總餐廳數不超過一半
                        if (isrestaurantD == 0 || isrestaurantA + isrestaurantB + isrestaurantC + restaurantCount < attractionCount / 2)
                        {
                            //5.景點E
                            var attractionTemp2 = locationCache
                                            .Where(a => a.Value.Distance(LocationD) < shortDistance
                                                         && id != a.Key
                                                         && !tempId.Contains(a.Key)
                                                         && !returnAttractionId.Contains(a.Key))
                                            .Select(a => a.Key);


                            foreach (int id2 in attractionTemp2)
                            {
                                isrestaurantE = attractions.Where(a => a.AttractionId == id2)
                                                           .Select(a => a.IsRestaurant).FirstOrDefault();

                                //D或E是餐廳，如果總餐廳已超過一半就只能是景點
                                if (isrestaurantE == 0 || (isrestaurantD == 0 && isrestaurantA + isrestaurantB + isrestaurantC + restaurantCount < attractionCount / 2))
                                {
                                    LocationE = locationCache[id2];
                                    tempId.Add(id);
                                    tempId.Add(id2);
                                    okE = false;
                                    break;
                                }
                            }
                        }
                        if (okE == false)
                        {
                            break;
                        }
                    }

                    if (okE)
                    {
                        tempId.Clear();
                        continue;
                    }
                    else if (realAttractionCount == 5)
                    {
                        break;
                    }










                    //6.景點F(FG近)
                    attractionTemp = locationCache
                                     .Where(a => shortDistance < a.Value.Distance(LocationE)
                                                && a.Value.Distance(LocationE) < longDistance
                                                && !tempId.Contains(a.Key)
                                                && !returnAttractionId.Contains(a.Key))
                                     .Select(a => a.Key);

                    foreach (int id in attractionTemp)
                    {
                        isrestaurantF = attractions.Where(a => a.AttractionId == id)
                                                   .Select(a => a.IsRestaurant).FirstOrDefault();
                        LocationF = locationCache[id];


                        //總餐廳數不超過一半
                        if (isrestaurantF == 0 || isrestaurantA + isrestaurantB + isrestaurantC + isrestaurantD + isrestaurantE + restaurantCount < attractionCount / 2)
                        {
                            //7.景點G
                            var attractionTemp2 = locationCache
                                            .Where(a => a.Value.Distance(LocationF) < shortDistance
                                                         && id != a.Key
                                                         && !tempId.Contains(a.Key)
                                                         && !returnAttractionId.Contains(a.Key))
                                            .Select(a => a.Key);


                            foreach (int id2 in attractionTemp2)
                            {
                                isrestaurantG = attractions.Where(a => a.AttractionId == id2)
                                                           .Select(a => a.IsRestaurant).FirstOrDefault();

                                //F或G是餐廳
                                if (isrestaurantG == 0 || isrestaurantF + isrestaurantG < 2)
                                {
                                    LocationG = locationCache[id2];
                                    tempId.Add(id);
                                    tempId.Add(id2);
                                    okG = false;
                                    break;
                                }
                            }
                        }
                        if (okG == false)
                        {
                            break;
                        }
                    }

                    if (okG)
                    {
                        tempId.Clear();
                        continue;
                    }
                    else if (realAttractionCount == 7)
                    {
                        break;
                    }
                }
            }


            //填入入回傳LIST
            foreach (int id in tempId)
            {
                returnAttractionId.Add(id);
            }


            ////最終輸出不得有景點不足的情況
            //if(returnAttractionId.Count < attractionCount)
            //{
            //    int need = attractionCount - returnAttractionId.Count;
            //    var needId = attractions.Where(a => !returnAttractionId.Contains(a.AttractionId)).Select(a => a.AttractionId).Take(need).ToList();
            //    returnAttractionId.AddRange(needId);
            //}



            string imgPath = "https://" + Request.RequestUri.Host + "/upload/AttractionImage/";
            if (returnAttractionId.Count == attractionCount)
            {
                List<object> results = new List<object>();

                foreach (int id in returnAttractionId)
                {
                    var result = _db.Images.Where(a => a.Attraction.AttractionId == id)
                                 .Select(a => new
                                 {
                                     AttractionId = id,
                                     AttractionName = a.Attraction.AttractionName,
                                     Elong = a.Attraction.Elong,
                                     Nlat = a.Attraction.Nlat,
                                     Lock = choose.AttractionId.Contains(id) ? true : false,
                                     ImageUrl = imgPath + a.ImageName
                                 }).FirstOrDefault();

                    results.Add(result);
                }

                return Ok(results);
            }
            else if (returnAttractionId.Count < attractionCount)
            {
                return BadRequest("景點數量不足");
            }
            else
            {
                return BadRequest("景點數量過多");
            }



        }




        /// <summary>
        ///     用戶修改原本行程按儲存-新建
        /// </summary>
        [HttpPost]
        [JwtAuthFilter]
        public IHttpActionResult TourAdd(TourAddView tourAdd)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuuid = (string)userToken["UserGuid"];
            int userId = _db.Users.Where(u => u.UserGuid == userGuuid).Select(u => u.UserId).FirstOrDefault();

            //新增行程
            Tour newTour = new Tour();

            if (tourAdd.TourName != "")
            {
                newTour.TourName = tourAdd.TourName;
            }
            else
            {
                newTour.TourName = "未命名行程";
            }
            newTour.UserId = userId;
            newTour.InitDate = DateTime.Now;

            _db.Tours.Add(newTour);
            _db.SaveChanges();

            //新增行程中的景點
            TourAttraction tourAttraction = new TourAttraction();
            int i = 1;
            foreach(int id in tourAdd.AttractionId)
            {
                tourAttraction.TourId = _db.Tours.Where(t => t.TourId == newTour.TourId).Select(t => t.TourId).FirstOrDefault();

                tourAttraction.AttractionId = id;
                tourAttraction.OrderNum = i;
                i++;

                _db.TourAttractions.Add(tourAttraction);
                _db.SaveChanges();
            }

            return Ok("新增行程成功");
        }







        /// <summary>
        ///     取得單一用戶收藏行程頁面
        /// </summary>
        [HttpGet]
        [Route("{tourId}")]
        public IHttpActionResult TourContent([FromUri] int tourId)
        {
            var tour= _db.Tours.Where(a => a.TourId == tourId).FirstOrDefault();

            if (tour!=null)
            {
                TourView result = new TourView();

                result.TourId = tourId;
                result.TourName = tour.TourName;

                var attractions = _db.TourAttractions.Where(a => a.TourId == tourId).OrderBy(a => a.OrderNum).ToList();
                result.Attractions = new List<object>();
                string imgPath = "https://" + Request.RequestUri.Host + "/upload/AttractionImage/";

                foreach (var attraction in attractions)
                {
                    var temp = _db.Images.Where(a => a.Attraction.AttractionId == attraction.AttractionId)
                                 .Select(a => new
                                 {
                                     AttractionId = attraction.AttractionId,
                                     AttractionName = a.Attraction.AttractionName,
                                     Elong = a.Attraction.Elong,
                                     Nlat = a.Attraction.Nlat,
                                     ImageUrl = imgPath + a.ImageName
                                 }).FirstOrDefault();

                    result.Attractions.Add(temp);
                }

                return Ok(result);
            }
            else
            {
                return BadRequest("查無行程");
            }
        }








        /// <summary>
        ///     按下分享取得行程景點
        /// </summary>
        [HttpGet]
        [Route("share")]
        public IHttpActionResult ShareTour([FromUri] List<int> id)
        {
            var result = new List<object>();
            string imgPath = "https://" + Request.RequestUri.Host + "/upload/AttractionImage/";

            //防止景點id亂打
            bool ok = true;

            foreach (int attractionId in id)
            {
                var attraction = _db.Attractions.Where(a => a.AttractionId == attractionId).FirstOrDefault();
                if (attraction != null)
                {
                    var temp = new
                    {
                        attraction.AttractionId,
                        attraction.AttractionName,
                        attraction.Elong,
                        attraction.Nlat,
                        ImageURL = imgPath + _db.Images.Where(i => i.AttractionId == attractionId).Select(i => i.ImageName).FirstOrDefault()
                    };

                    result.Add(temp);
                }
                else
                {
                    ok = false;
                    break;
                }
            }

            if(ok)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest("景點id有誤");
            }
        }











        /// <summary>
        ///     地理資料測試
        /// </summary>
        [HttpGet]
        [Route("test")]
        public IHttpActionResult Testgeo()
        {
            DbGeography A = _db.Attractions.Where(a => a.AttractionId == 33).Select(a => a.Location).FirstOrDefault();
            DbGeography B = _db.Attractions.Where(a => a.AttractionId == 34).Select(a => a.Location).FirstOrDefault();
            double result = (double)A.Distance(B);
            return Ok(result);
        }
    }
}
