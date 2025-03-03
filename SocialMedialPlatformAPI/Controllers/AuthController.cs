using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SocialMedialPlatformAPI.Common;
using SocialMedialPlatformAPI.DTO;
using SocialMedialPlatformAPI.Helpers;
using SocialMedialPlatformAPI.Interface;
using SocialMedialPlatformAPI.Models;
using SocialMedialPlatformAPI.Utils;

namespace SocialMedialPlatformAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IValidationService _validationService;
        private readonly ResponseHandler _responseHandler;
        private readonly IAuthService _authService;
        private readonly Helper _helper;

        public AuthController(IValidationService validationService,ResponseHandler responseHandler,IAuthService authService,Helper helper)
        {
            _validationService = validationService;
            _responseHandler = responseHandler;
            _authService = authService;
            _helper = helper;
        }

        [HttpPost("Registration")]
        public async Task<ActionResult> UserRegistation([FromBody]UserDto userDto)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateRegistration(userDto);
                if(errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid,CustomErrorMessage.ValidationRegistration,errors));
                }

                UserDto? user = await _authService.InsertUserData(userDto);
                if(user==null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsRegister,CustomErrorMessage.RegistrationError, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.RegistrationSucces, user));

            }
            catch(SqlException exp)
            {
                throw new Exception("Sql Exception While Registration:"+exp.Message);
            }
            catch (Exception ex) 
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsRegister, ex.Message, ""));
            }
        }

        [HttpPost("Login")]
        public async Task<ActionResult> UserLogin([FromBody]LoginRequestDto loginRequestDto)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateLogin(loginRequestDto);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationLogin, ""));
                }
                LoginResponseDto loginResponseDto = await _authService.LoginUser(loginRequestDto);
                if (string.IsNullOrEmpty(loginResponseDto.Token))
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.LoginError, CustomErrorMessage.InvalidUsernameOrPassword, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.LoginSucces, loginResponseDto));
            }
            catch (SqlException exp)
            {
                throw new Exception("SqlException While Login" + exp.Message);
            }
            catch (Exception ex) 
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.LoginError,ex.Message,""));
            }
        }

        [HttpPost("ForgetPassword")]
        public async Task<ActionResult> ForgetPassword([FromBody]ForgetPasswordDto forgetPasswordDto)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateForgetPassword(forgetPasswordDto);
                if(errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.InvalidEmailFormat,CustomErrorMessage.InvalidEmailFormat,errors));
                }
                User user = await _authService.GetUser(forgetPasswordDto);
                byte[] b = System.Text.ASCIIEncoding.ASCII.GetBytes(user.UserId.ToString());
                string encryptedUserId = Convert.ToBase64String(b);

                string subject = "Forgot Password - Instagram";
                string resetLink = $"https://e828-202-131-123-10.ngrok-free.app/resetpassword/{encryptedUserId}";

                string htmlMessage = $@"
                                    <html>
                        <body style=""font-family: Arial, sans-serif; background-color:rgb(243, 242, 242);  padding: 20px;"">
 
                            <!-- Header -->
                            <div style="" padding: 10px; text-align: center;"">
                                <img src=""https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRNFFDufYJlSyMP1NgyV8OUR_zYH9YIcCcCUA&s""  style=""width: 120px; height: auto;"">
                            </div>
                            <div style="" padding-left: 15px; border-radius: 5px; margin-top: 20px;"">
                                <p>Hi <span style=""color: #0095f6;"">{user.Name}</span>,</p>
                                <p>Sorry to hear you’re having trouble logging into Instagram. We got a message that you forgot your password. If this was you, you can get right back into your account or reset your password now.</p>
                                <div style=""text-align: center; margin-top: 20px;"">
                                    <br>
                                    <a href=""{resetLink}"" style=""display: inline-block; background-color: #0095f6; color: white; text-decoration: none; padding: 10px 20px; border-radius: 5px;"">Reset your password</a>
                                </div>
                                <p style=""margin-top: 20px;"">If you didn’t request a login link or a password reset, you can ignore this message and <a href=""#"" style=""color: #0095f6; text-decoration: none;"">learn more about why you may have received it.</a></p>
                                <p>Only people who know your Instagram password or click the login link in this email can log into your account.</p>
                            </div>
                        </body>
                        </html>
                        ";
                // Send email using EmailSender method
                if (!await _helper.EmailSender(user.Email ?? string.Empty, subject, htmlMessage))
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.MailNotSend, CustomErrorMessage.MailNotSend, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.MailSend,forgetPasswordDto));

            }
            catch (SqlException exp)
            {
                throw new Exception("SqlExeception While ForgetPassword"+exp.Message);
            }
            catch(Exception ex)
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsNotExist,ex.Message,""));
            }
        }

        [HttpPost("ForgetPasswordUpdate")]
        public async Task<ActionResult> ForgetPasswordUpdate([FromBody]ForgetPasswordDto forgetPasswordDto)
        {
            try
            {
                byte[] b = Convert.FromBase64String(forgetPasswordDto.EncyptUserId ?? string.Empty.ToString());
                string dcryptedUserId = System.Text.ASCIIEncoding.ASCII.GetString(b);
                forgetPasswordDto.UserId = Convert.ToInt32(dcryptedUserId);

                List<ValidationError> errors = _validationService.ValidateForgetPasswordData(forgetPasswordDto);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationReset, errors));
                }

                bool IsData = await _authService.ForgotPassword(forgetPasswordDto);

                if (!IsData)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsReset, CustomErrorMessage.ResetError, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.ForgotPassword,forgetPasswordDto));
            }
            catch (SqlException exp)
            {
                throw new Exception("SqlExecption while ForgetPasswordUpdate"+exp.Message);
            }
            catch(Exception ex)
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsReset,CustomErrorMessage.ResetError,ex.Message));
            }
        }

        [HttpPost("ResetPassword")]
        [Authorize]
        public async Task<ActionResult> ResetPassword([FromBody]ResetPasswordRequestDto resetPasswordRequestDto)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateResetPassword(resetPasswordRequestDto);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationReset, errors));
                }
                bool IsData = await _authService.ResetPassword(resetPasswordRequestDto);

                if (!IsData)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsReset, CustomErrorMessage.ResetError, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.ReserPassword, resetPasswordRequestDto));
            }
            catch (SqlException ex)
            {
                throw new Exception("ResetPassword while Occured"+ex.Message);
            }
            catch
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsReset, CustomErrorMessage.ResetError, ""));
            }
           
        }

    }
}
