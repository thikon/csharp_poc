using Lightwork.License.Package.EnumType;
using Lightwork.License.Package.Environment;
using Lightwork.License.Package.Helper;
using Lightwork.License.Package.Helper.Interface;
using Lightwork.License.Package.Model;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Management;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace TestInsertRegistry
{
    class Program
    {
        static void Main(string[] args)
        {
            //var register = new PocRegistry(null);
            //string key = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJDdXN0b21lckNvZGUiOiJUUU0iLCJFeHBpcmVUeXBlIjoiMiIsIkV4cGlyZURhdGUiOiIzMCIsIkFwcGxpY2F0aW9uVHlwZSI6IjIiLCJMaWNlbnNlT25saW5lVHlwZSI6IjEiLCJDcmVhdGVEYXRlVGltZSI6IjIwMjEtMTEtMTEgMTE6NDc6NDQuNTcxIiwibmJmIjoxNjM2NjA2MDY0LCJleHAiOjE2MzY3Nzg4NjQsImlhdCI6MTYzNjYwNjA2NH0.gJ7mJ4RWJFWWwYPhLXoUXD8uP-jp-9l9ZFkFTX3XhLM";

            try
            {
                string DATEFORMAT = "yyyy-MM-dd";
                string TIMEFORMAT = @"hh\:mm\:ss";

                var tmp = DateTime.ParseExact("2021-11-18", DATEFORMAT, null);
                var tmp1 = TimeSpan.ParseExact("02:00:00", TIMEFORMAT, null);

                //var start = DateTime.Now;
                //var compare = new DateTime(2021,12,12, 12,0,0);

                //var temp = (compare - start).TotalDays;
                //Console.WriteLine(temp);

                //Console.WriteLine($"case 1,, { register.GetMotherBoardID() == null }");

                //Console.WriteLine($"case 2,, {"".Equals(register.GetMotherBoardID())}");

                //Console.WriteLine("key");
                //Console.WriteLine(register.ReadRegistry());


                // register.InsertRegistry(key);
                //Console.WriteLine();
                //string uuid = !string.IsNullOrEmpty(register.GetMotherBoardID()) ? register.GetMotherBoardID() : "no uuid";
                //Console.WriteLine($"MainBoard UUID: {uuid}");

                //Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadKey();
        }
    }

    class PocRegistry
    {
        private readonly string _secret = "D1lL4UzLxZMqECdd6jc9hl79tDeeCU9H1+6O3JQBRDPqQw8TTT+X84vmuePCmffpVY5r1NnMYC8jS2PXNdtmffrFmmNpzVkmtCEUIgVGewZh7mzDK6nCrDb6WcU4WYmSUVrmHoMwXH3klmreR8H3xNyHWNFoppYYMRbqaJKU5qc=";
        private readonly string _encryptKey = "bFpxOgC3z+pqv681XKkQnlTsaQA1lU73Z9TIx3gxkvvQD/4ti8Jd4Oc4zuXDATY1kya1rqM7K14sMw4GA66Aj+iiN5sCdhS/MBp/9n9jqgwI2KejJ1bOlYk1IeU7ol3ACp7qxbZW+uv6BOdARP5xaf+VYbngk880HC8CzfP4dmJ2eOxNRid5AghYT5wezBoGwkfCyv5WL0uV5X/hzypoqRi//svVIflynWT1hOR5uAKI50YDzV4VVg==";

        const string FormatDate = "yyyy-MM-dd";
        private readonly IIdentityHelper _identityHelper;
        private readonly IRegistryService _registryService;

        public PocRegistry()
        {

        }

        public PocRegistry(IIdentityHelper identityService)
        {
            _identityHelper = identityService ?? new IdentityHelper();
        }

        public string ReadRegistry()
        {
            var ident = _identityHelper.ReadRegistry(ApplicationType.Creator);
            return ident?.IdentityMain?.OldToken;
        }

        public void InsertRegistry(string key)
        {
            string tempTokenAfterActivate = string.Empty;
            Console.WriteLine("1#");
            var identity = _identityHelper.GetIdentityActivate(key);

            Console.WriteLine("2#");
            var infoAfterActivate = GenExpireDate(identity);
            if (!string.IsNullOrEmpty(infoAfterActivate.CustomerCode))
            {
                Console.WriteLine("3.1#");
                DateTime tempExpireDate = DateTime.Parse(infoAfterActivate.ExpireDate);
                Console.WriteLine(tempExpireDate.ToString());
                Console.WriteLine("3.2#");
                DateTime tempActivateDate = DateTime.Parse(infoAfterActivate.ActivateDate);
                Console.WriteLine(tempActivateDate.ToString());
                Console.WriteLine("3.3#");
                if (tempExpireDate < tempActivateDate)
                {
                    return;
                }
            }

            Console.WriteLine("4#");
            tempTokenAfterActivate = GenerateToken(infoAfterActivate);
            Console.WriteLine("5#");
            Console.WriteLine(tempTokenAfterActivate);
            _identityHelper.GenerateFileKey(RegistryCreatorFile.KeyFileName, tempTokenAfterActivate);
        }

        private IdentityInfoAfterActivate GenExpireDate(IdentityInfo identity)
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US", false);

            DateTime tempActivateDate = DateTime.Now;

            IdentityInfoAfterActivate identityInfoAfter = new IdentityInfoAfterActivate()
            {
                CustomerCode = identity.CustomerCode,
                ExpireType = identity.ExpireType,
                ExpireDateFromKey = identity.ExpireDate,
                OldToken = identity.Token,
                ActivateDate = tempActivateDate.ToString(FormatDate),
                MainBoardUUID = _identityHelper.GetMotherBoardID(),
            };

            if (identity.ExpireType.Equals(ExpireType.FixExpire))
            {
                identityInfoAfter.ExpireDate = DateTime.Parse(identity.ExpireDate).ToString(FormatDate);
            }
            else if (identity.ExpireType.Equals(ExpireType.AfterActivate))
            {
                identityInfoAfter.ExpireDate = tempActivateDate.AddDays(double.Parse(identity.ExpireDate)).ToString(FormatDate);// identityInfoAfter.ActivateDate.AddDays(double.Parse(identity.ExpireDate)).ToString("yyyy-MM-dd");
            }
            else if (identity.ExpireType.Equals(ExpireType.Infinity))
            {
                identityInfoAfter.ExpireDateFromKey = DateTime.MaxValue.ToString(FormatDate);
                identityInfoAfter.ExpireDate = DateTime.MaxValue.ToString(FormatDate);
                identityInfoAfter.OldToken = "YeaH Dev";
            }

            return identityInfoAfter;
        }

        public string GenerateToken(IdentityInfoAfterActivate identity)
        {
            string tempToken = string.Empty;
            if (identity == null) return tempToken; 

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            Console.WriteLine($"SecretKey: {securityKey}");
            Console.WriteLine($"CustomerCode: {identity.CustomerCode}");
            Console.WriteLine($"ExpireType: {((int)identity.ExpireType).ToString()}");
            Console.WriteLine($"ExpireDateFromKey: { identity.ExpireDateFromKey}");
            Console.WriteLine($"ExpireDate: {identity.ExpireDate}");
            Console.WriteLine($"ActivateDate: { identity.ActivateDate}");
            Console.WriteLine($"MainBoardUUID: {identity.MainBoardUUID}");
            Console.WriteLine($"OldToken: {identity.OldToken}");
            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                    new[] {
                            new Claim(ClaimTypeCustom.CustomerCode, identity.CustomerCode),
                            new Claim(ClaimTypeCustom.ExpireType, ((int)identity.ExpireType).ToString()),
                            new Claim(ClaimTypeCustom.ExpireDateFromKey, identity.ExpireDateFromKey),
                            new Claim(ClaimTypeCustom.ExpireDate, identity.ExpireDate),
                            new Claim(ClaimTypeCustom.ActivateDate, identity.ActivateDate),
                            new Claim(ClaimTypeCustom.MainBoardUUID, identity.MainBoardUUID ?? string.Empty),
                            new Claim(ClaimTypeCustom.Token, identity.OldToken)
                    }
                ),
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
            };
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.CreateJwtSecurityToken(descriptor);
            tempToken = handler.WriteToken(jwtToken);

            return tempToken;

        }

        public string GetMotherBoardID()
        {
            try
            {
                string mbInfo = String.Empty;
                mbInfo = GetMotherBoardUUID();
                if (string.IsNullOrEmpty(mbInfo))
                {
                    ManagementScope scope = new ManagementScope($"\\\\{System.Environment.MachineName}\\root\\cimv2");
                    scope.Connect();
                    ManagementObject wmiClass = new ManagementObject(scope, new ManagementPath("Win32_BaseBoard.Tag=\"Base Board\""), new ObjectGetOptions());
                    foreach (PropertyData propData in wmiClass.Properties)
                    {
                        if (propData.Name == "SerialNumber")
                            mbInfo = String.Format("{0,-25}{1}", propData.Name, Convert.ToString(propData.Value));
                        if (!string.IsNullOrEmpty(mbInfo))
                            break;
                    }
                }
                return mbInfo;
            }
            catch
            {
                return null;
            }

            string GetMotherBoardUUID()
            {
                try
                {
                    string uuid = string.Empty;
                    ManagementClass mc = new ManagementClass("Win32_ComputerSystemProduct");
                    ManagementObjectCollection moc = mc.GetInstances();
                    return moc.Cast<ManagementObject>().FirstOrDefault(x => x.Properties.Cast<PropertyData>().Any(p => p.Name == "UUID"))?
                         .Properties["UUID"]?.Value.ToString();
                }
                catch
                {
                    return string.Empty;
                }
            }

        }

        public object GetIdentity(string key, TokenType tokenType)
        {
            try
            {

                string authorizationHeader = key;
                if (authorizationHeader == null) return null;

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = authorizationHeader.Replace("Bearer ", "");
                var paresedToken = tokenHandler.ReadJwtToken(token);

                if (paresedToken == null) return null;
                var encryptSecret = _encryptKey;
                var secret = SecurityHelper.DecryptString(encryptSecret);
                var parameters = new TokenValidationParameters()
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = tokenType.Equals(TokenType.IdentityBeforeActivate) ? true : false,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
                };

                ClaimsPrincipal principal = tokenHandler.ValidateToken(token, parameters, out SecurityToken securityToken);
                if (principal == null) return null;

                var info = new object();
                if (tokenType.Equals(TokenType.IdentityBeforeActivate))
                {
                    IdentityInfo identityInfo = new IdentityInfo()
                    {
                        Token = token,
                        CustomerCode = GetClaimValue("CustomerCode", paresedToken),
                        ExpireDate = GetClaimValue("ExpireDate", paresedToken)
                    };

                    int.TryParse(GetClaimValue("ApplicationType", paresedToken), out int applicationType);
                    identityInfo.ApplicationType = Enum.IsDefined(typeof(ApplicationType), applicationType)
                        ? (ApplicationType)applicationType : ApplicationType.Robot;

                    int.TryParse(GetClaimValue("LicenseOnlineType", paresedToken), out int licenseOnlineType);
                    identityInfo.LicenseOnlineType = Enum.IsDefined(typeof(LicenseOnlineType), licenseOnlineType)
                        ? (LicenseOnlineType)licenseOnlineType : LicenseOnlineType.Offline;

                    int.TryParse(GetClaimValue("ExpireType", paresedToken), out int expireType);
                    identityInfo.ExpireType = Enum.IsDefined(typeof(ExpireType), expireType)
                        ? (ExpireType)expireType : ExpireType.AfterActivate;

                    info = identityInfo;
                }
                else if (tokenType.Equals(TokenType.IdentityAfterActivate))
                {
                    IdentityInfoAfterActivate identityInfoAfterActivate = new IdentityInfoAfterActivate()
                    {
                        OldToken = token,
                        CustomerCode = GetClaimValue("CustomerCode", paresedToken),
                        ExpireDate = GetClaimValue("ExpireDate", paresedToken),
                        ExpireDateFromKey = GetClaimValue("ExpireDateFromKey", paresedToken),
                        ActivateDate = GetClaimValue("ActivateDate", paresedToken),
                        MainBoardUUID = GetClaimValue("MainBoardUUID", paresedToken),

                    };

                    //int.TryParse(paresedToken.Claims.FirstOrDefault(c => c.Type.Equals("LicenseOnlineType", StringComparison.CurrentCultureIgnoreCase)).Value, out int licenseOnlineType);
                    //identityInfoAfterActivate.LicenseOnlineType = Enum.IsDefined(typeof(LicenseOnlineType), licenseOnlineType)
                    //    ? (LicenseOnlineType)licenseOnlineType : LicenseOnlineType.Offline;

                    int.TryParse(GetClaimValue("ExpireType", paresedToken), out int expireType);
                    identityInfoAfterActivate.ExpireType = GetExpireType(expireType);

                    info = identityInfoAfterActivate;
                }
                else if (tokenType.Equals(TokenType.IdentityTimeLatest))
                {
                    IdentityActiveTimeLatest identityActiveTimeLatest = new IdentityActiveTimeLatest()
                    {
                        Token = token
                    };
                    DateTime.TryParse(GetClaimValue("ActiveTimeLatest", paresedToken), out DateTime activeTimeLatest);
                    identityActiveTimeLatest.ActiveTimeLatest = activeTimeLatest;

                    info = identityActiveTimeLatest;
                }

                return info;
            }
            catch (Exception ex)
            {
                throw ex;
                // return null;
            }

        }
        public string GetClaimValue(string key, JwtSecurityToken jwtSecurity)
        {
            return jwtSecurity?.Claims?.FirstOrDefault(c => c.Type.Equals(key, StringComparison.CurrentCultureIgnoreCase))?.Value ?? string.Empty;
        }
        public ExpireType GetExpireType(int temp)
        {
            ExpireType expireType = ExpireType.FixExpire;

            switch (temp)
            {
                case (int)ExpireType.AfterActivate:

                    expireType = ExpireType.AfterActivate;
                    break;

                case (int)ExpireType.FixExpire:

                    expireType = ExpireType.FixExpire;
                    break;

                case (int)ExpireType.Infinity:

                    expireType = ExpireType.Infinity;
                    break;
            }

            return expireType;
        }
        public void GenerateFileKey(string fileName, string text)
        {
            //IRegistryService registryService = _endpoint.CreateChannel<IRegistryService>();
            ////------------------------------------------------------------------------
            //registryService.Create(fileName, text);
        }
    }
}
