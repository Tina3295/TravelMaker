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
    [RoutePrefix("api/tours")]
    public class ToursController : ApiController
    {
        private TravelMakerDbContext _db = new TravelMakerDbContext();

        /// <summary>
        ///     隨機產生行程(含鎖點擴充)
        /// </summary>
        [HttpPost]
        [Route("make")]
        public IHttpActionResult Make(GetToursView choose)
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


            //最終輸出不得有景點不足的情況
            if (returnAttractionId.Count < attractionCount)
            {
                int need = attractionCount - returnAttractionId.Count;
                var needId = attractions.Where(a => !returnAttractionId.Contains(a.AttractionId)).Select(a => a.AttractionId).Take(need).ToList();
                returnAttractionId.AddRange(needId);
            }



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
            string userGuid = (string)userToken["UserGuid"];
            int userId = _db.Users.Where(u => u.UserGuid == userGuid).Select(u => u.UserId).FirstOrDefault();

          
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
            tourAttraction.TourId = _db.Tours.Where(t => t.TourId == newTour.TourId).Select(t => t.TourId).FirstOrDefault();
            int i = 1;
            foreach(int id in tourAdd.AttractionId)
            {
                tourAttraction.AttractionId = id;
                tourAttraction.OrderNum = i;
                i++;

                _db.TourAttractions.Add(tourAttraction);
                _db.SaveChanges();
            }

            return Ok(new { Message = "新增行程成功", TourId = tourAttraction.TourId });
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
                result.UserGuid = _db.Users.Where(u => u.UserId == tour.UserId).Select(u => u.UserGuid).FirstOrDefault();//用戶識別碼
                    

                var attractions = _db.TourAttractions.Where(a => a.TourId == tourId).OrderBy(a => a.OrderNum).ToList();
                result.Attractions = new List<object>();
                string imgPath = "https://" + Request.RequestUri.Host + "/upload/attractionImage/";

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
                        ImageUrl = imgPath + _db.Images.Where(i => i.AttractionId == attractionId).Select(i => i.ImageName).FirstOrDefault()
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
        ///     用戶複製行程
        /// </summary>
        [HttpPost]
        [JwtAuthFilter]
        [Route("{tourId}/duplicate")]
        public IHttpActionResult TourDuplicate([FromUri]int tourId,[FromBody]string TourName)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = (string)userToken["UserGuid"];
            int userId = _db.Users.Where(u => u.UserGuid == userGuid).Select(u => u.UserId).FirstOrDefault();

            //複製行程-實際上增加一筆紀錄
            Tour newTour = new Tour();

            if (TourName != "")
            {
                newTour.TourName = TourName;
            }
            else
            {
                newTour.TourName = "未命名行程";
            }
            newTour.UserId = userId;
            newTour.InitDate = DateTime.Now;

            _db.Tours.Add(newTour);
            _db.SaveChanges();

            //複製行程中的景點
            var attractions = _db.TourAttractions.Where(t => t.TourId == tourId).OrderBy(a => a.OrderNum).Select(a => a.AttractionId).ToList();

            TourAttraction tourAttraction = new TourAttraction();
            int i = 1;
            foreach (int id in attractions)
            {
                tourAttraction.TourId = _db.Tours.Where(t => t.TourId == newTour.TourId).Select(t => t.TourId).FirstOrDefault();

                tourAttraction.AttractionId = id;
                tourAttraction.OrderNum = i;
                i++;

                _db.TourAttractions.Add(tourAttraction);
                _db.SaveChanges();
            }

            return Ok(new { Message = "複製行程成功" });
        }




        /// <summary>
        ///     用戶修改原本行程按儲存-覆蓋
        /// </summary>
        [HttpPost]
        [JwtAuthFilter]
        [Route("{tourId}/modify")]
        public IHttpActionResult TourModify([FromUri] int tourId, TourAddView tourView)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = (string)userToken["UserGuid"];
            int userId = _db.Users.Where(u => u.UserGuid == userGuid).Select(u => u.UserId).FirstOrDefault();
            //是否為該行程擁有者
            var tour = _db.Tours.Where(t => t.TourId == tourId).FirstOrDefault();

            if(tour.UserId== userId)
            {
                //tour記錄-更改tour名字
                tour.TourName = tourView.TourName;
                _db.SaveChanges();

                //刪除原本的行程景點
                var originAttractions = _db.TourAttractions.Where(t => t.TourId == tourId).ToList();
                foreach (var originAttraction in originAttractions)
                {
                    _db.TourAttractions.Remove(originAttraction);
                    _db.SaveChanges();
                }

                //寫入新景點
                TourAttraction tourAttraction = new TourAttraction();
                int i = 1;
                foreach (int id in tourView.AttractionId)
                {
                    tourAttraction.TourId = tourId;
                    tourAttraction.AttractionId = id;
                    tourAttraction.OrderNum = i;
                    i++;

                    _db.TourAttractions.Add(tourAttraction);
                    _db.SaveChanges();
                }

                //愛心數歸零
                var likes = _db.TourLikes.Where(l => l.TourId == tourId).ToList();
                foreach(var like in likes)
                {
                    _db.TourLikes.Remove(like);
                    _db.SaveChanges();
                }

                return Ok(new { Message = "行程修改成功" });
            }
            else
            {
                return BadRequest("非該行程擁有者");
            }

        }



        /// <summary>
        ///     刪除自己的行程
        /// </summary>
        [HttpDelete]
        [JwtAuthFilter]
        [Route("{tourId}")]
        public IHttpActionResult TourDelete([FromUri]int tourId)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = userToken["UserGuid"].ToString();
            int userId = _db.Users.Where(u => u.UserGuid == userGuid).Select(u => u.UserId).FirstOrDefault();

            //是否為該行程擁有者
            var tour = _db.Tours.Where(t => t.TourId == tourId).FirstOrDefault();
            if (tour.UserId==userId)
            {
                //刪除行程景點
                var attractions = _db.TourAttractions.Where(a => a.TourId == tourId).ToList();
                foreach(var attraction in attractions)
                {
                    _db.TourAttractions.Remove(attraction);
                }
                //刪除愛心記錄
                var likes = _db.TourLikes.Where(a => a.TourId == tourId).ToList();
                foreach (var like in likes)
                {
                    _db.TourLikes.Remove(like);
                }
                //刪除行程
                _db.Tours.Remove(tour);
                _db.SaveChanges();

                return Ok(new { Message = "刪除成功" });
            }
            else
            {
                return BadRequest("非該行程擁有者");
            }
        }




        /// <summary>
        ///     給參數搜尋行程(熱門話題取得所有行程)
        /// </summary>
        [HttpGet]
        [Route("search")]
        public IHttpActionResult ToursSearch(string type="", string district="", string keyword="", int page = 1)
        {
            int pageSize = 9;
            string imgPath = "https://" + Request.RequestUri.Host + "/upload/AttractionImage/";
            int myUserId = 0;
            if (Request.Headers.Authorization != null)
            {
                var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
                string userGuid = (string)userToken["UserGuid"];
                myUserId = _db.Users.Where(u => u.UserGuid == userGuid).Select(u => u.UserId).FirstOrDefault();
            }


            //行程搜尋篩選
            var temp = _db.TourAttractions.AsQueryable();

            if (!string.IsNullOrEmpty(district))
            {
                temp = temp.Where(t => t.Attraction.District.DistrictName == district);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                temp = temp.Where(t => t.Attraction.AttractionName.Contains(keyword) || t.Attraction.Introduction.Contains(keyword) || t.Attraction.Address.Contains(keyword) || t.Tour.TourName.Contains(keyword));
            }

            if (!string.IsNullOrEmpty(type))
            {
                var attractions = _db.CategoryAttractions.Where(a => a.Category.CategoryName == type).Select(a => a.AttractionId).Distinct().ToList();

                temp = temp.Where(t => attractions.Contains(t.AttractionId));
            }

            //符合搜尋結果的總項目
            int totalItem = temp.Select(t => t.TourId).Distinct().Count();
            //總頁數
            int totalPages = totalItem % pageSize == 0 ? totalItem / pageSize : totalItem / pageSize + 1;
            //該頁顯示項目
            var searchTours = temp.Select(t => new
            {
                TourId = t.TourId,
                Likes = _db.TourLikes.Count(l => l.TourId == t.TourId)
            }).Distinct().OrderByDescending(t => t.Likes).Skip(pageSize * (page - 1)).Take(pageSize).ToList();

            var result = new List<object>();

            foreach (var searchTour in searchTours)
            {
                TourSearch tour = new TourSearch();
                tour.TourId = searchTour.TourId;
                tour.TourName = _db.Tours.Where(t => t.TourId == searchTour.TourId).Select(t => t.TourName).FirstOrDefault();
                tour.AttrCounts = _db.TourAttractions.Where(t => t.TourId == searchTour.TourId).Count();
                tour.Likes = searchTour.Likes;

                tour.ImageUrl = new List<string>();
                var attractionIds = _db.TourAttractions.Where(t => t.TourId == searchTour.TourId).Select(t => t.AttractionId).ToList();
                for (int i = 0; i < Math.Min(attractionIds.Count, 3); i++)
                {
                    int attractionId = attractionIds[i];
                    string img = _db.Images.Where(a => a.AttractionId == attractionId).Select(a => a.ImageName).FirstOrDefault();

                    tour.ImageUrl.Add(imgPath + img);
                }

                if (myUserId != 0)
                {
                    tour.IsLike = _db.TourLikes.Where(t => t.TourId == searchTour.TourId).Any(t => t.UserId == myUserId) ? true : false;
                }
                else
                {
                    tour.IsLike = false;
                }
                
                result.Add(tour);
            }


            
            if (totalItem != 0)
            {
                return Ok(new { TotalPages = totalPages, TotalItem = totalItem, Tours = result });
            }
            else
            {
                return BadRequest("尚無符合搜尋條件的行程");
            }
        }



        /// <summary>
        ///     試玩行程
        /// </summary>
        [HttpGet]
        [Route("try")]
        public IHttpActionResult TourTry()
        {
            //隨機類別
            List<int> types = new List<int>();
            Random random = new Random();
            int categoryId = random.Next(1, 8);
            types.Add(categoryId);
            
            if (!types.Contains(4))
            {
                types.Add(8);
            }
            else
            {
                types.Add(9);
            }

            //隨機選區
            int district = _db.Districts.Select(a => a.DistrictId).OrderBy(x => Guid.NewGuid()).FirstOrDefault();

            //景點清單
            var attractions = _db.CategoryAttractions.Where(a => a.Attraction.OpenStatus == true
                                                             && types.Contains(a.CategoryId)
                                                             && a.Attraction.DistrictId == district)
                              .Select(a => new AttractionList
                              {
                                  AttractionId = a.Attraction.AttractionId,
                                  IsRestaurant = a.CategoryId == 8 || a.CategoryId == 9 ? 1 : 0
                              })
                              .Distinct().OrderBy(x => Guid.NewGuid()).ToList();

            //兩景點兩餐廳
            var fourAttractionIds = new List<int>();
            for (int i = 0; i < 2; i++)
            {
                fourAttractionIds.Add(attractions.Where(a => a.IsRestaurant == 0 && !fourAttractionIds.Contains(a.AttractionId)).Select(a => a.AttractionId).FirstOrDefault());
                fourAttractionIds.Add(attractions.Where(a => a.IsRestaurant == 1 && !fourAttractionIds.Contains(a.AttractionId)).Select(a => a.AttractionId).FirstOrDefault());
            }


            TourTryView tourTry = new TourTryView();
            string imgPath = "https://" + Request.RequestUri.Host + "/upload/AttractionImage/";

            tourTry.Category = _db.Categories.Where(c => c.CategoryId == categoryId).Select(c => c.CategoryName).FirstOrDefault();

            tourTry.AttractionData = new List<object>();
            foreach(int id in fourAttractionIds)
            {
                var attraction = _db.Attractions.Where(a => a.AttractionId == id).Select(a => new
                {
                    a.AttractionId,
                    a.AttractionName,
                    ImageUrl =imgPath+_db.Images.Where(i=>i.AttractionId==id).Select(i=>i.ImageName).FirstOrDefault()
                });

                tourTry.AttractionData.Add(attraction);
            }

            return Ok(tourTry);
        }





        /// <summary>
        ///     用戶把行程按愛心
        /// </summary>
        /// <param name="tourId">行程Id</param>
        /// <returns></returns>
        [HttpPost]
        [JwtAuthFilter]
        [Route("{tourId}/like")]
        public IHttpActionResult TourLike([FromUri]int tourId)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = (string)userToken["UserGuid"];
            int userId = _db.Users.Where(u => u.UserGuid == userGuid).Select(u => u.UserId).FirstOrDefault();

            var like = _db.TourLikes.Where(a => a.UserId == userId && a.TourId == tourId).FirstOrDefault();
            if (like == null)
            {
                TourLike tourLike = new TourLike();
                tourLike.TourId = tourId;
                tourLike.UserId = userId;
                tourLike.InitDate = DateTime.Now;
                _db.TourLikes.Add(tourLike);
                _db.SaveChanges();
                return Ok();
            }
            else
            {
                return BadRequest("已按過喜歡");
            }
        }


        /// <summary>
        ///      用戶取消行程愛心
        /// </summary>
        /// <param name="tourId">行程Id</param>
        /// <returns></returns>
        [HttpDelete]
        [JwtAuthFilter]
        [Route("{tourId}/like")]
        public IHttpActionResult TourUnlike([FromUri] int tourId)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = (string)userToken["UserGuid"];
            int userId = _db.Users.Where(u => u.UserGuid == userGuid).Select(u => u.UserId).FirstOrDefault();

            var like = _db.TourLikes.Where(a => a.UserId == userId && a.TourId == tourId).FirstOrDefault();
            if (like != null)
            {
                _db.TourLikes.Remove(like);
                _db.SaveChanges();
            }
            return Ok();
        }





        /// <summary>
        ///     首頁-取得熱門行程*4、取得熱門景點*3
        /// </summary>
        [HttpGet]
        [Route("homepage")]
        public IHttpActionResult TourHomePage()
        {
            string imgPath = "https://" + Request.RequestUri.Host + "/upload/AttractionImage/";
            int myUserId = 0;
            if (Request.Headers.Authorization != null)
            {
                var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
                string userGuid = (string)userToken["UserGuid"];
                myUserId = _db.Users.Where(u => u.UserGuid == userGuid).Select(u => u.UserId).FirstOrDefault();
            }
            HomePageView result = new HomePageView();

            //取得熱門行程*4 (固定四個景點、愛心數>100)          
            var tours = _db.TourLikes.GroupBy(t => t.TourId).Where(t => _db.TourAttractions.Where(a => a.TourId == t.Key).Count() == 4 && t.Count() > 100).Select(t => new
            {
                TourId = t.Key,
                Likes =t.Count()
            }).OrderBy(x => Guid.NewGuid()).Take(4).ToList();

            result.Tours = new List<HomePageTour>();
            foreach (var t in tours)
            {
                //行程資訊
                HomePageTour tour = new HomePageTour();
                tour.TourId = t.TourId;
                tour.TourName = _db.Tours.Where(x => x.TourId == t.TourId).Select(x => x.TourName).FirstOrDefault();

                tour.ImageUrl = imgPath + _db.Images.Where(i => i.AttractionId == _db.TourAttractions.Where(x => x.TourId == t.TourId).Select(x => x.AttractionId).FirstOrDefault()).Select(i => i.ImageName).FirstOrDefault();

                var attractionIds = _db.TourAttractions.Where(x => x.TourId == t.TourId).Select(x => x.AttractionId).ToList();
                tour.Category = _db.CategoryAttractions.Where(c => attractionIds.Contains(c.AttractionId) && c.CategoryId != 8 && c.CategoryId != 9).Select(c => c.Category.CategoryName).FirstOrDefault();
                if(tour.Category==null)
                {
                    tour.Category = "吃貨行程";
                }

                tour.Likes = t.Likes;
                if (myUserId != 0)
                {
                    tour.IsLike = _db.TourLikes.Where(x => x.TourId == t.TourId).Any(x => x.UserId == myUserId) ? true : false;
                }
                else
                {
                    tour.IsLike = false;
                }


                //行程中的景點資訊
                tour.Attractions = new List<HomePageTourAttraction>();

                foreach (var id in attractionIds)
                {
                    HomePageTourAttraction attraction = new HomePageTourAttraction();
                    var att = _db.Attractions.Where(a => a.AttractionId == id).FirstOrDefault();
                    attraction.AttractionName = att.AttractionName;
                    attraction.Elong = Convert.ToDouble(att.Elong);
                    attraction.Nlat = Convert.ToDouble(att.Nlat);
                    tour.Attractions.Add(attraction);
                }
                result.Tours.Add(tour);
            }


            //取得熱門景點*3
            var attractions = _db.AttractionComments.GroupBy(a => a.AttractionId).Where(a => a.Average(s => s.Score) > 4).Select(a => new
            {
                AttractionId = a.Key,
                AverageScore = a.Average(s => s.Score)
            })
                .OrderBy(x => Guid.NewGuid()).Take(3).ToList();

            result.Attractions = new List<AttractionView>();
            foreach(var att in attractions)
            {
                AttractionView attraction = new AttractionView();
                attraction.AttractionId = att.AttractionId;
                attraction.AttractionName = _db.Attractions.Where(a => a.AttractionId == att.AttractionId).Select(x => x.AttractionName).FirstOrDefault();

                attraction.CityDistrict = _db.Attractions.Where(x => x.AttractionId == att.AttractionId).Select(x => x.District.City.CittyName).FirstOrDefault()
                    + " " + _db.Attractions.Where(x => x.AttractionId == att.AttractionId).Select(x => x.District.DistrictName).FirstOrDefault();
                attraction.AverageScore = (int)Math.Round( att.AverageScore);

                attraction.Category = _db.CategoryAttractions.Where(c => c.AttractionId == att.AttractionId && c.CategoryId != 8 && c.CategoryId != 9).Select(c => c.Category.CategoryName).ToList();
                if (attraction.Category.Count == 0)
                {
                    attraction.Category.Add("餐廳");
                }

                attraction.ImageUrl = imgPath + _db.Images.Where(x => x.AttractionId == att.AttractionId).Select(x => x.ImageName).FirstOrDefault();

                if (myUserId != 0)
                {
                    attraction.IsCollect = _db.AttractionCollections.Where(a => a.AttractionId == att.AttractionId).Any(a => a.UserId == myUserId) ? true : false;
                }
                else
                {
                    attraction.IsCollect = false;
                }

                result.Attractions.Add(attraction);
            }


            return Ok(result);
        }
    }
}
