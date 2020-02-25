using DigitalOceanAuth.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EasyNetQ;
using DigitalOceanBot.BusMessage;
using System;
using DigitalOceanBot.MongoDb;
using RestSharp;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DigitalOceanBot.MongoDb.Models;

namespace DigitalOceanAuth.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IBus _bus;
        private readonly IRepository<DoUser> _userRepo;

        public AuthController(ILogger<AuthController> logger, IRepository<DoUser> userRepo, IBus bus)
        {
            _logger = logger;
            _userRepo = userRepo;
            _bus = bus;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery]AuthRequest request)
        {
            try
            {
                _logger.LogInformation($"Start request. Code={request.Code}, State={request.State}");

                if (string.IsNullOrEmpty(request.Error))
                {
                    var userId = GetUserIdFromRequestState(request.State);

                    if (userId == 0)
                    {
                        _bus.Send("auth-queue", new AuthMessage
                        {
                            IsSuccess = false
                        });

                        return View(new AuthModel { IsSuccess = false });
                    }

                    var user = _userRepo.Get(userId);

                    if (user == null)
                    {
                        _bus.Send("auth-queue", new AuthMessage
                        {
                            IsSuccess = false
                        });

                        return View(new AuthModel { IsSuccess = false });
                    }

                    if (!CheckUserState(request.State, user.State))
                    {
                        _bus.Send("auth-queue", new AuthMessage
                        {
                            IsSuccess = false
                        });

                        return View(new AuthModel { IsSuccess = false });
                    }

                    var userInfo = await GetToken(request.Code, request.State).ConfigureAwait(false);

                    if (string.IsNullOrEmpty(userInfo.error))
                    {
                        _userRepo.Update(user.UserId, (user) =>
                        {
                            user.TokenExpires = DateTime.UtcNow.AddMinutes(15);
                            user.Token = userInfo.access_token;
                            user.IsAuthorized = true;
                        });

                        _bus.Send("auth-queue", new AuthMessage
                        {
                            IsSuccess = true,
                            UserId = userId,
                            ChatId = userId
                        });

                        _logger.LogInformation($"Auth success. UserId={userId.ToString()}");

                        return View(new AuthModel { IsSuccess = true });
                    }
                    else
                    {
                        _bus.Send("auth-queue", new AuthMessage
                        {
                            IsSuccess = false
                        });

                        _logger.LogError($"Code={request.Code}, State={request.State}, UserId={user.UserId.ToString()}, Error={userInfo.error_description}");

                        return View(new AuthModel { IsSuccess = false });
                    }
                }
                else
                {
                    _logger.LogError($"Code={request.Code}, State={request.State}, Error={request.ErrorDescription}");

                    _bus.Send("auth-queue", new AuthMessage
                    {
                        IsSuccess = false
                    });

                    return View(new AuthModel { IsSuccess = false });
                }
            }
            catch(Exception ex)
            {
                _logger.LogError($"Code={request.Code}, State={request.State}, ErrorMessage={ex.Message}, StackTrace={ex.StackTrace}");

                _bus.Send("auth-queue", new AuthMessage
                {
                    IsSuccess = false
                });

                return View(new AuthModel { IsSuccess = false });
            }
            finally
            {
                _bus.Dispose();
            }
        }

        private async Task<UserInfo> GetToken(string code, string state)
        {
            try
            {
                var url = Environment.GetEnvironmentVariable("URL");
                url += $"&client_id={Environment.GetEnvironmentVariable("CLIENT_ID")}";
                url += $"&client_secret={Environment.GetEnvironmentVariable("SECRET")}";
                url += $"&code={code}";
                url += $"&redirect_uri={Environment.GetEnvironmentVariable("REDIRECT_URI")}";

                var client = new RestClient(url)
                {
                    Timeout = -1
                };
                
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");
                var response = await client.ExecuteAsync(request);
                return JsonConvert.DeserializeObject<UserInfo>(response.Content);

            }
            catch (Exception ex)
            {
                _logger.LogError($"Code={code}, State={state}, Error={ex.Message}, StackTrace={ex.StackTrace}");

                return new UserInfo
                {
                    error = ex.Message
                };
            }
        }

        private bool CheckUserState(string requestState, string userState)
        {
            try
            {
                var requestStateFromBase64 = Convert.FromBase64String(requestState);
                var requestStateDecoded = Encoding.UTF8.GetString(requestStateFromBase64);
                var requestStateArray = requestStateDecoded.Split(';');

                var userStateFromBase64 = Convert.FromBase64String(userState);
                var userStateDecoded = Encoding.UTF8.GetString(userStateFromBase64);
                var userStateArray = userStateDecoded.Split(';');

                if (requestStateArray.Length != 3)
                {
                    return false;
                }

                if (userStateArray[0] != requestStateArray[0])
                {
                    return false;
                }

                return true;
            }
            catch(Exception ex)
            {
                _logger.LogError($"Check state error. RequestState={requestState}, UserState={userState}, Error={ex.Message}");
                return false;
            }
        }

        private int GetUserIdFromRequestState(string requestState)
        {
            try
            {
                var requestStateFromBase64 = Convert.FromBase64String(requestState);
                var requestStateDecoded = Encoding.UTF8.GetString(requestStateFromBase64);
                var requestStateArray = requestStateDecoded.Split(';');

                return int.Parse(requestStateArray[1]);
            }
            catch(Exception ex)
            {
                _logger.LogError($"Parse error UserId. RequestState={requestState}, Error={ex.Message}");
                return 0;
            }
        }
    }
}
