; ModuleID = 'marshal_methods.x86_64.ll'
source_filename = "marshal_methods.x86_64.ll"
target datalayout = "e-m:e-p270:32:32-p271:32:32-p272:64:64-i64:64-f80:128-n8:16:32:64-S128"
target triple = "x86_64-unknown-linux-android21"

%struct.MarshalMethodName = type {
	i64, ; uint64_t id
	ptr ; char* name
}

%struct.MarshalMethodsManagedClass = type {
	i32, ; uint32_t token
	ptr ; MonoClass klass
}

@assembly_image_cache = dso_local local_unnamed_addr global [357 x ptr] zeroinitializer, align 16

; Each entry maps hash of an assembly name to an index into the `assembly_image_cache` array
@assembly_image_cache_hashes = dso_local local_unnamed_addr constant [1071 x i64] [
	i64 u0x001e58127c546039, ; 0: lib_System.Globalization.dll.so => 44
	i64 u0x0024d0f62dee05bd, ; 1: Xamarin.KotlinX.Coroutines.Core.dll => 314
	i64 u0x0071cf2d27b7d61e, ; 2: lib_Xamarin.AndroidX.SwipeRefreshLayout.dll.so => 292
	i64 u0x0075ea86498d3e4d, ; 3: OpenAI => 213
	i64 u0x01109b0e4d99e61f, ; 4: System.ComponentModel.Annotations.dll => 15
	i64 u0x02123411c4e01926, ; 5: lib_Xamarin.AndroidX.Navigation.Runtime.dll.so => 282
	i64 u0x022e81ea9c46e03a, ; 6: lib_CommunityToolkit.Maui.Core.dll.so => 179
	i64 u0x022f31be406de945, ; 7: Microsoft.Extensions.Options.ConfigurationExtensions => 203
	i64 u0x0284512fad379f7e, ; 8: System.Runtime.Handles => 107
	i64 u0x02abedc11addc1ed, ; 9: lib_Mono.Android.Runtime.dll.so => 173
	i64 u0x02f55bf70672f5c8, ; 10: lib_System.IO.FileSystem.DriveInfo.dll.so => 50
	i64 u0x032267b2a94db371, ; 11: lib_Xamarin.AndroidX.AppCompat.dll.so => 238
	i64 u0x03621c804933a890, ; 12: System.Buffers => 9
	i64 u0x0363ac97a4cb84e6, ; 13: SQLitePCLRaw.provider.e_sqlite3.dll => 225
	i64 u0x0399610510a38a38, ; 14: lib_System.Private.DataContractSerialization.dll.so => 88
	i64 u0x043032f1d071fae0, ; 15: ru/Microsoft.Maui.Controls.resources => 342
	i64 u0x044440a55165631e, ; 16: lib-cs-Microsoft.Maui.Controls.resources.dll.so => 320
	i64 u0x046eb1581a80c6b0, ; 17: vi/Microsoft.Maui.Controls.resources => 348
	i64 u0x0470607fd33c32db, ; 18: Microsoft.IdentityModel.Abstractions.dll => 207
	i64 u0x047408741db2431a, ; 19: Xamarin.AndroidX.DynamicAnimation => 258
	i64 u0x0517ef04e06e9f76, ; 20: System.Net.Primitives => 73
	i64 u0x0565d18c6da3de38, ; 21: Xamarin.AndroidX.RecyclerView => 285
	i64 u0x057bf9fa9fb09f7c, ; 22: Microsoft.Data.Sqlite.dll => 185
	i64 u0x0581db89237110e9, ; 23: lib_System.Collections.dll.so => 14
	i64 u0x05989cb940b225a9, ; 24: Microsoft.Maui.dll => 210
	i64 u0x05a1c25e78e22d87, ; 25: lib_System.Runtime.CompilerServices.Unsafe.dll.so => 104
	i64 u0x05ef98b6a1db882c, ; 26: lib_Microsoft.Data.Sqlite.dll.so => 185
	i64 u0x06076b5d2b581f08, ; 27: zh-HK/Microsoft.Maui.Controls.resources => 349
	i64 u0x06388ffe9f6c161a, ; 28: System.Xml.Linq.dll => 158
	i64 u0x06600c4c124cb358, ; 29: System.Configuration.dll => 21
	i64 u0x067f95c5ddab55b3, ; 30: lib_Xamarin.AndroidX.Fragment.Ktx.dll.so => 263
	i64 u0x0680a433c781bb3d, ; 31: Xamarin.AndroidX.Collection.Jvm => 245
	i64 u0x069fff96ec92a91d, ; 32: System.Xml.XPath.dll => 163
	i64 u0x070b0847e18dab68, ; 33: Xamarin.AndroidX.Emoji2.ViewsHelper.dll => 260
	i64 u0x0739448d84d3b016, ; 34: lib_Xamarin.AndroidX.VectorDrawable.dll.so => 295
	i64 u0x07469f2eecce9e85, ; 35: mscorlib.dll => 169
	i64 u0x07c57877c7ba78ad, ; 36: ru/Microsoft.Maui.Controls.resources.dll => 342
	i64 u0x07dcdc7460a0c5e4, ; 37: System.Collections.NonGeneric => 12
	i64 u0x08122e52765333c8, ; 38: lib_Microsoft.Extensions.Logging.Debug.dll.so => 201
	i64 u0x088610fc2509f69e, ; 39: lib_Xamarin.AndroidX.VectorDrawable.Animated.dll.so => 296
	i64 u0x08881a0a9768df86, ; 40: lib_Azure.Core.dll.so => 176
	i64 u0x08a7c865576bbde7, ; 41: System.Reflection.Primitives => 98
	i64 u0x08c9d051a4a817e5, ; 42: Xamarin.AndroidX.CustomView.PoolingContainer.dll => 256
	i64 u0x08f3c9788ee2153c, ; 43: Xamarin.AndroidX.DrawerLayout => 257
	i64 u0x09138715c92dba90, ; 44: lib_System.ComponentModel.Annotations.dll.so => 15
	i64 u0x0919c28b89381a0b, ; 45: lib_Microsoft.Extensions.Options.dll.so => 202
	i64 u0x092266563089ae3e, ; 46: lib_System.Collections.NonGeneric.dll.so => 12
	i64 u0x095cacaf6b6a32e4, ; 47: System.Memory.Data => 227
	i64 u0x09d144a7e214d457, ; 48: System.Security.Cryptography => 129
	i64 u0x09e2b9f743db21a8, ; 49: lib_System.Reflection.Metadata.dll.so => 97
	i64 u0x0a805f95d98f597b, ; 50: lib_Microsoft.Extensions.Caching.Abstractions.dll.so => 189
	i64 u0x0abb3e2b271edc45, ; 51: System.Threading.Channels.dll => 142
	i64 u0x0b06b1feab070143, ; 52: System.Formats.Tar => 41
	i64 u0x0b3b632c3bbee20c, ; 53: sk/Microsoft.Maui.Controls.resources => 343
	i64 u0x0b6aff547b84fbe9, ; 54: Xamarin.KotlinX.Serialization.Core.Jvm => 317
	i64 u0x0be2e1f8ce4064ed, ; 55: Xamarin.AndroidX.ViewPager => 298
	i64 u0x0c3ca6cc978e2aae, ; 56: pt-BR/Microsoft.Maui.Controls.resources => 339
	i64 u0x0c59ad9fbbd43abe, ; 57: Mono.Android => 174
	i64 u0x0c65741e86371ee3, ; 58: lib_Xamarin.Android.Glide.GifDecoder.dll.so => 232
	i64 u0x0c74af560004e816, ; 59: Microsoft.Win32.Registry.dll => 7
	i64 u0x0c7790f60165fc06, ; 60: lib_Microsoft.Maui.Essentials.dll.so => 211
	i64 u0x0c83c82812e96127, ; 61: lib_System.Net.Mail.dll.so => 69
	i64 u0x0cce4bce83380b7f, ; 62: Xamarin.AndroidX.Security.SecurityCrypto => 289
	i64 u0x0d13cd7cce4284e4, ; 63: System.Security.SecureString => 132
	i64 u0x0d3b5ab8b2766190, ; 64: lib_Microsoft.Bcl.AsyncInterfaces.dll.so => 184
	i64 u0x0d63f4f73521c24f, ; 65: lib_Xamarin.AndroidX.SavedState.SavedState.Ktx.dll.so => 288
	i64 u0x0e04e702012f8463, ; 66: Xamarin.AndroidX.Emoji2 => 259
	i64 u0x0e14e73a54dda68e, ; 67: lib_System.Net.NameResolution.dll.so => 70
	i64 u0x0ec01b05613190b9, ; 68: SkiaSharp.Views.Android.dll => 218
	i64 u0x0f37dd7a62ae99af, ; 69: lib_Xamarin.AndroidX.Collection.Ktx.dll.so => 246
	i64 u0x0f5e7abaa7cf470a, ; 70: System.Net.HttpListener => 68
	i64 u0x1001f97bbe242e64, ; 71: System.IO.UnmanagedMemoryStream => 59
	i64 u0x1028057492a97bb5, ; 72: IdeaBranch.App.dll => 2
	i64 u0x102861e4055f511a, ; 73: Microsoft.Bcl.AsyncInterfaces.dll => 184
	i64 u0x102a31b45304b1da, ; 74: Xamarin.AndroidX.CustomView => 255
	i64 u0x1065c4cb554c3d75, ; 75: System.IO.IsolatedStorage.dll => 54
	i64 u0x10f6cfcbcf801616, ; 76: System.IO.Compression.Brotli => 45
	i64 u0x114443cdcf2091f1, ; 77: System.Security.Cryptography.Primitives => 127
	i64 u0x11a603952763e1d4, ; 78: System.Net.Mail => 69
	i64 u0x11a70d0e1009fb11, ; 79: System.Net.WebSockets.dll => 83
	i64 u0x11f26371eee0d3c1, ; 80: lib_Xamarin.AndroidX.Lifecycle.Runtime.Ktx.dll.so => 273
	i64 u0x12128b3f59302d47, ; 81: lib_System.Xml.Serialization.dll.so => 160
	i64 u0x123639456fb056da, ; 82: System.Reflection.Emit.Lightweight.dll => 94
	i64 u0x12521e9764603eaa, ; 83: lib_System.Resources.Reader.dll.so => 101
	i64 u0x125b7f94acb989db, ; 84: Xamarin.AndroidX.RecyclerView.dll => 285
	i64 u0x12d3b63863d4ab0b, ; 85: lib_System.Threading.Overlapped.dll.so => 143
	i64 u0x134eab1061c395ee, ; 86: System.Transactions => 153
	i64 u0x138567fa954faa55, ; 87: Xamarin.AndroidX.Browser => 242
	i64 u0x13a01de0cbc3f06c, ; 88: lib-fr-Microsoft.Maui.Controls.resources.dll.so => 326
	i64 u0x13beedefb0e28a45, ; 89: lib_System.Xml.XmlDocument.dll.so => 164
	i64 u0x13f1e5e209e91af4, ; 90: lib_Java.Interop.dll.so => 171
	i64 u0x13f1e880c25d96d1, ; 91: he/Microsoft.Maui.Controls.resources => 327
	i64 u0x143d8ea60a6a4011, ; 92: Microsoft.Extensions.DependencyInjection.Abstractions => 194
	i64 u0x1497051b917530bd, ; 93: lib_System.Net.WebSockets.dll.so => 83
	i64 u0x14d612a531c79c05, ; 94: Xamarin.JSpecify.dll => 309
	i64 u0x14e68447938213b7, ; 95: Xamarin.AndroidX.Collection.Ktx.dll => 246
	i64 u0x152a448bd1e745a7, ; 96: Microsoft.Win32.Primitives => 6
	i64 u0x1557de0138c445f4, ; 97: lib_Microsoft.Win32.Registry.dll.so => 7
	i64 u0x15bdc156ed462f2f, ; 98: lib_System.IO.FileSystem.dll.so => 53
	i64 u0x15e300c2c1668655, ; 99: System.Resources.Writer.dll => 103
	i64 u0x16bf2a22df043a09, ; 100: System.IO.Pipes.dll => 58
	i64 u0x16ea2b318ad2d830, ; 101: System.Security.Cryptography.Algorithms => 122
	i64 u0x16eeae54c7ebcc08, ; 102: System.Reflection.dll => 100
	i64 u0x17125c9a85b4929f, ; 103: lib_netstandard.dll.so => 170
	i64 u0x1716866f7416792e, ; 104: lib_System.Security.AccessControl.dll.so => 120
	i64 u0x174f71c46216e44a, ; 105: Xamarin.KotlinX.Coroutines.Core => 314
	i64 u0x1752c12f1e1fc00c, ; 106: System.Core => 23
	i64 u0x17b56e25558a5d36, ; 107: lib-hu-Microsoft.Maui.Controls.resources.dll.so => 330
	i64 u0x17f9358913beb16a, ; 108: System.Text.Encodings.Web => 139
	i64 u0x1805f780a2be57b5, ; 109: Polly.Core.dll => 215
	i64 u0x1809fb23f29ba44a, ; 110: lib_System.Reflection.TypeExtensions.dll.so => 99
	i64 u0x18402a709e357f3b, ; 111: lib_Xamarin.KotlinX.Serialization.Core.Jvm.dll.so => 317
	i64 u0x18a9befae51bb361, ; 112: System.Net.WebClient => 79
	i64 u0x18f0ce884e87d89a, ; 113: nb/Microsoft.Maui.Controls.resources.dll => 336
	i64 u0x19777fba3c41b398, ; 114: Xamarin.AndroidX.Startup.StartupRuntime.dll => 291
	i64 u0x19a4c090f14ebb66, ; 115: System.Security.Claims => 121
	i64 u0x1a6fceea64859810, ; 116: Azure.Identity => 177
	i64 u0x1a91866a319e9259, ; 117: lib_System.Collections.Concurrent.dll.so => 10
	i64 u0x1aac34d1917ba5d3, ; 118: lib_System.dll.so => 167
	i64 u0x1aad60783ffa3e5b, ; 119: lib-th-Microsoft.Maui.Controls.resources.dll.so => 345
	i64 u0x1aea8f1c3b282172, ; 120: lib_System.Net.Ping.dll.so => 72
	i64 u0x1b4b7a1d0d265fa2, ; 121: Xamarin.Android.Glide.DiskLruCache => 231
	i64 u0x1bbdb16cfa73e785, ; 122: Xamarin.AndroidX.Lifecycle.Runtime.Ktx.Android => 274
	i64 u0x1bc766e07b2b4241, ; 123: Xamarin.AndroidX.ResourceInspection.Annotation.dll => 286
	i64 u0x1c292b1598348d77, ; 124: Microsoft.Extensions.Diagnostics.dll => 195
	i64 u0x1c753b5ff15bce1b, ; 125: Mono.Android.Runtime.dll => 173
	i64 u0x1cd47467799d8250, ; 126: System.Threading.Tasks.dll => 147
	i64 u0x1d23eafdc6dc346c, ; 127: System.Globalization.Calendars.dll => 42
	i64 u0x1da4110562816681, ; 128: Xamarin.AndroidX.Security.SecurityCrypto.dll => 289
	i64 u0x1db6820994506bf5, ; 129: System.IO.FileSystem.AccessControl.dll => 49
	i64 u0x1dbb0c2c6a999acb, ; 130: System.Diagnostics.StackTrace => 32
	i64 u0x1e3d87657e9659bc, ; 131: Xamarin.AndroidX.Navigation.UI => 283
	i64 u0x1e71143913d56c10, ; 132: lib-ko-Microsoft.Maui.Controls.resources.dll.so => 334
	i64 u0x1e7c31185e2fb266, ; 133: lib_System.Threading.Tasks.Parallel.dll.so => 146
	i64 u0x1ed8fcce5e9b50a0, ; 134: Microsoft.Extensions.Options.dll => 202
	i64 u0x1f055d15d807e1b2, ; 135: System.Xml.XmlSerializer => 165
	i64 u0x1f1ed22c1085f044, ; 136: lib_System.Diagnostics.FileVersionInfo.dll.so => 30
	i64 u0x1f61df9c5b94d2c1, ; 137: lib_System.Numerics.dll.so => 86
	i64 u0x1f750bb5421397de, ; 138: lib_Xamarin.AndroidX.Tracing.Tracing.dll.so => 293
	i64 u0x20237ea48006d7a8, ; 139: lib_System.Net.WebClient.dll.so => 79
	i64 u0x209375905fcc1bad, ; 140: lib_System.IO.Compression.Brotli.dll.so => 45
	i64 u0x20fab3cf2dfbc8df, ; 141: lib_System.Diagnostics.Process.dll.so => 31
	i64 u0x2110167c128cba15, ; 142: System.Globalization => 44
	i64 u0x21419508838f7547, ; 143: System.Runtime.CompilerServices.VisualC => 105
	i64 u0x2174319c0d835bc9, ; 144: System.Runtime => 119
	i64 u0x2198e5bc8b7153fa, ; 145: Xamarin.AndroidX.Annotation.Experimental.dll => 236
	i64 u0x2199f06354c82d3b, ; 146: System.ClientModel.dll => 226
	i64 u0x219ea1b751a4dee4, ; 147: lib_System.IO.Compression.ZipFile.dll.so => 47
	i64 u0x21cc7e445dcd5469, ; 148: System.Reflection.Emit.ILGeneration => 93
	i64 u0x220fd4f2e7c48170, ; 149: th/Microsoft.Maui.Controls.resources => 345
	i64 u0x224538d85ed15a82, ; 150: System.IO.Pipes => 58
	i64 u0x22908438c6bed1af, ; 151: lib_System.Threading.Timer.dll.so => 150
	i64 u0x237be844f1f812c7, ; 152: System.Threading.Thread.dll => 148
	i64 u0x23852b3bdc9f7096, ; 153: System.Resources.ResourceManager => 102
	i64 u0x23986dd7e5d4fc01, ; 154: System.IO.FileSystem.Primitives.dll => 51
	i64 u0x2407aef2bbe8fadf, ; 155: System.Console => 22
	i64 u0x240abe014b27e7d3, ; 156: Xamarin.AndroidX.Core.dll => 251
	i64 u0x2460a7609cc9be7a, ; 157: Microsoft.Extensions.AI.OpenAI.dll => 188
	i64 u0x247619fe4413f8bf, ; 158: System.Runtime.Serialization.Primitives.dll => 116
	i64 u0x24de8d301281575e, ; 159: Xamarin.Android.Glide => 229
	i64 u0x252073cc3caa62c2, ; 160: fr/Microsoft.Maui.Controls.resources.dll => 326
	i64 u0x256b8d41255f01b1, ; 161: Xamarin.Google.Crypto.Tink.Android => 304
	i64 u0x25a0a7eff76ea08e, ; 162: SQLitePCLRaw.batteries_v2.dll => 222
	i64 u0x2662c629b96b0b30, ; 163: lib_Xamarin.Kotlin.StdLib.dll.so => 310
	i64 u0x268c1439f13bcc29, ; 164: lib_Microsoft.Extensions.Primitives.dll.so => 204
	i64 u0x26a670e154a9c54b, ; 165: System.Reflection.Extensions.dll => 96
	i64 u0x26d077d9678fe34f, ; 166: System.IO.dll => 60
	i64 u0x273f3515de5faf0d, ; 167: id/Microsoft.Maui.Controls.resources.dll => 331
	i64 u0x2742545f9094896d, ; 168: hr/Microsoft.Maui.Controls.resources => 329
	i64 u0x2759af78ab94d39b, ; 169: System.Net.WebSockets => 83
	i64 u0x27b2b16f3e9de038, ; 170: Xamarin.Google.Crypto.Tink.Android.dll => 304
	i64 u0x27b410442fad6cf1, ; 171: Java.Interop.dll => 171
	i64 u0x27b97e0d52c3034a, ; 172: System.Diagnostics.Debug => 28
	i64 u0x2801845a2c71fbfb, ; 173: System.Net.Primitives.dll => 73
	i64 u0x286835e259162700, ; 174: lib_Xamarin.AndroidX.ProfileInstaller.ProfileInstaller.dll.so => 284
	i64 u0x28e52865585a1ebe, ; 175: Microsoft.Extensions.Diagnostics.Abstractions.dll => 196
	i64 u0x2927d345f3daec35, ; 176: SkiaSharp.dll => 217
	i64 u0x2949f3617a02c6b2, ; 177: Xamarin.AndroidX.ExifInterface => 261
	i64 u0x2a128783efe70ba0, ; 178: uk/Microsoft.Maui.Controls.resources.dll => 347
	i64 u0x2a3b095612184159, ; 179: lib_System.Net.NetworkInformation.dll.so => 71
	i64 u0x2a6507a5ffabdf28, ; 180: System.Diagnostics.TraceSource.dll => 35
	i64 u0x2ad156c8e1354139, ; 181: fi/Microsoft.Maui.Controls.resources => 325
	i64 u0x2ad5d6b13b7a3e04, ; 182: System.ComponentModel.DataAnnotations.dll => 16
	i64 u0x2af298f63581d886, ; 183: System.Text.RegularExpressions.dll => 141
	i64 u0x2afc1c4f898552ee, ; 184: lib_System.Formats.Asn1.dll.so => 40
	i64 u0x2afc670d2becf718, ; 185: Microsoft.Extensions.AI.OpenAI => 188
	i64 u0x2b148910ed40fbf9, ; 186: zh-Hant/Microsoft.Maui.Controls.resources.dll => 351
	i64 u0x2b6989d78cba9a15, ; 187: Xamarin.AndroidX.Concurrent.Futures.dll => 247
	i64 u0x2c8bd14bb93a7d82, ; 188: lib-pl-Microsoft.Maui.Controls.resources.dll.so => 338
	i64 u0x2cbd9262ca785540, ; 189: lib_System.Text.Encoding.CodePages.dll.so => 136
	i64 u0x2cc9e1fed6257257, ; 190: lib_System.Reflection.Emit.Lightweight.dll.so => 94
	i64 u0x2cd723e9fe623c7c, ; 191: lib_System.Private.Xml.Linq.dll.so => 90
	i64 u0x2d169d318a968379, ; 192: System.Threading.dll => 151
	i64 u0x2d47774b7d993f59, ; 193: sv/Microsoft.Maui.Controls.resources.dll => 344
	i64 u0x2d5ffcae1ad0aaca, ; 194: System.Data.dll => 26
	i64 u0x2db915caf23548d2, ; 195: System.Text.Json.dll => 140
	i64 u0x2dcaa0bb15a4117a, ; 196: System.IO.UnmanagedMemoryStream.dll => 59
	i64 u0x2e5a40c319acb800, ; 197: System.IO.FileSystem => 53
	i64 u0x2e6f1f226821322a, ; 198: el/Microsoft.Maui.Controls.resources.dll => 323
	i64 u0x2f02f94df3200fe5, ; 199: System.Diagnostics.Process => 31
	i64 u0x2f2e98e1c89b1aff, ; 200: System.Xml.ReaderWriter => 159
	i64 u0x2f5911d9ba814e4e, ; 201: System.Diagnostics.Tracing => 36
	i64 u0x2f84070a459bc31f, ; 202: lib_System.Xml.dll.so => 166
	i64 u0x2feb4d2fcda05cfd, ; 203: Microsoft.Extensions.Caching.Abstractions.dll => 189
	i64 u0x2ff49de6a71764a1, ; 204: lib_Microsoft.Extensions.Http.dll.so => 197
	i64 u0x309ee9eeec09a71e, ; 205: lib_Xamarin.AndroidX.Fragment.dll.so => 262
	i64 u0x309f2bedefa9a318, ; 206: Microsoft.IdentityModel.Abstractions => 207
	i64 u0x30c6dda129408828, ; 207: System.IO.IsolatedStorage => 54
	i64 u0x31195fef5d8fb552, ; 208: _Microsoft.Android.Resource.Designer.dll => 356
	i64 u0x312c8ed623cbfc8d, ; 209: Xamarin.AndroidX.Window.dll => 300
	i64 u0x31496b779ed0663d, ; 210: lib_System.Reflection.DispatchProxy.dll.so => 92
	i64 u0x315f08d19390dc36, ; 211: Xamarin.Google.ErrorProne.TypeAnnotations => 306
	i64 u0x31962e0f7c634c16, ; 212: Polly.Core => 215
	i64 u0x32243413e774362a, ; 213: Xamarin.AndroidX.CardView.dll => 243
	i64 u0x3235427f8d12dae1, ; 214: lib_System.Drawing.Primitives.dll.so => 37
	i64 u0x326256f7722d4fe5, ; 215: SkiaSharp.Views.Maui.Controls.dll => 219
	i64 u0x329753a17a517811, ; 216: fr/Microsoft.Maui.Controls.resources => 326
	i64 u0x32aa989ff07a84ff, ; 217: lib_System.Xml.ReaderWriter.dll.so => 159
	i64 u0x33829542f112d59b, ; 218: System.Collections.Immutable => 11
	i64 u0x33a31443733849fe, ; 219: lib-es-Microsoft.Maui.Controls.resources.dll.so => 324
	i64 u0x341abc357fbb4ebf, ; 220: lib_System.Net.Sockets.dll.so => 78
	i64 u0x3496c1e2dcaf5ecc, ; 221: lib_System.IO.Pipes.AccessControl.dll.so => 57
	i64 u0x34dfd74fe2afcf37, ; 222: Microsoft.Maui => 210
	i64 u0x34e292762d9615df, ; 223: cs/Microsoft.Maui.Controls.resources.dll => 320
	i64 u0x3508234247f48404, ; 224: Microsoft.Maui.Controls => 208
	i64 u0x353590da528c9d22, ; 225: System.ComponentModel.Annotations => 15
	i64 u0x3549870798b4cd30, ; 226: lib_Xamarin.AndroidX.ViewPager2.dll.so => 299
	i64 u0x355282fc1c909694, ; 227: Microsoft.Extensions.Configuration => 190
	i64 u0x3552fc5d578f0fbf, ; 228: Xamarin.AndroidX.Arch.Core.Common => 240
	i64 u0x355c649948d55d97, ; 229: lib_System.Runtime.Intrinsics.dll.so => 111
	i64 u0x35ea9d1c6834bc8c, ; 230: Xamarin.AndroidX.Lifecycle.ViewModel.Ktx.dll => 277
	i64 u0x3628ab68db23a01a, ; 231: lib_System.Diagnostics.Tools.dll.so => 34
	i64 u0x3673b042508f5b6b, ; 232: lib_System.Runtime.Extensions.dll.so => 106
	i64 u0x36740f1a8ecdc6c4, ; 233: System.Numerics => 86
	i64 u0x36b2b50fdf589ae2, ; 234: System.Reflection.Emit.Lightweight => 94
	i64 u0x36cada77dc79928b, ; 235: System.IO.MemoryMappedFiles => 55
	i64 u0x374ef46b06791af6, ; 236: System.Reflection.Primitives.dll => 98
	i64 u0x376bf93e521a5417, ; 237: lib_Xamarin.Jetbrains.Annotations.dll.so => 308
	i64 u0x37bc29f3183003b6, ; 238: lib_System.IO.dll.so => 60
	i64 u0x380134e03b1e160a, ; 239: System.Collections.Immutable.dll => 11
	i64 u0x38049b5c59b39324, ; 240: System.Runtime.CompilerServices.Unsafe => 104
	i64 u0x385c17636bb6fe6e, ; 241: Xamarin.AndroidX.CustomView.dll => 255
	i64 u0x38869c811d74050e, ; 242: System.Net.NameResolution.dll => 70
	i64 u0x393c226616977fdb, ; 243: lib_Xamarin.AndroidX.ViewPager.dll.so => 298
	i64 u0x395e37c3334cf82a, ; 244: lib-ca-Microsoft.Maui.Controls.resources.dll.so => 319
	i64 u0x39721dd6cab9d79e, ; 245: Polly.dll => 214
	i64 u0x3ab5859054645f72, ; 246: System.Security.Cryptography.Primitives.dll => 127
	i64 u0x3ad75090c3fac0e9, ; 247: lib_Xamarin.AndroidX.ResourceInspection.Annotation.dll.so => 286
	i64 u0x3ae44ac43a1fbdbb, ; 248: System.Runtime.Serialization => 118
	i64 u0x3b860f9932505633, ; 249: lib_System.Text.Encoding.Extensions.dll.so => 137
	i64 u0x3c3aafb6b3a00bf6, ; 250: lib_System.Security.Cryptography.X509Certificates.dll.so => 128
	i64 u0x3c4049146b59aa90, ; 251: System.Runtime.InteropServices.JavaScript => 108
	i64 u0x3c7c495f58ac5ee9, ; 252: Xamarin.Kotlin.StdLib => 310
	i64 u0x3c7e5ed3d5db71bb, ; 253: System.Security => 133
	i64 u0x3cd9d281d402eb9b, ; 254: Xamarin.AndroidX.Browser.dll => 242
	i64 u0x3d1c50cc001a991e, ; 255: Xamarin.Google.Guava.ListenableFuture.dll => 307
	i64 u0x3d2b1913edfc08d7, ; 256: lib_System.Threading.ThreadPool.dll.so => 149
	i64 u0x3d46f0b995082740, ; 257: System.Xml.Linq => 158
	i64 u0x3d8a8f400514a790, ; 258: Xamarin.AndroidX.Fragment.Ktx.dll => 263
	i64 u0x3d9c2a242b040a50, ; 259: lib_Xamarin.AndroidX.Core.dll.so => 251
	i64 u0x3da7781d6333a8fe, ; 260: SQLitePCLRaw.batteries_v2 => 222
	i64 u0x3dbb6b9f5ab90fa7, ; 261: lib_Xamarin.AndroidX.DynamicAnimation.dll.so => 258
	i64 u0x3e5441657549b213, ; 262: Xamarin.AndroidX.ResourceInspection.Annotation => 286
	i64 u0x3e57d4d195c53c2e, ; 263: System.Reflection.TypeExtensions => 99
	i64 u0x3e616ab4ed1f3f15, ; 264: lib_System.Data.dll.so => 26
	i64 u0x3e707b1acaaea668, ; 265: lib_Polly.Extensions.Http.dll.so => 216
	i64 u0x3f1d226e6e06db7e, ; 266: Xamarin.AndroidX.SlidingPaneLayout.dll => 290
	i64 u0x3f3c8f45ab6f28c7, ; 267: Microsoft.Identity.Client.Extensions.Msal.dll => 206
	i64 u0x3f510adf788828dd, ; 268: System.Threading.Tasks.Extensions => 145
	i64 u0x407a10bb4bf95829, ; 269: lib_Xamarin.AndroidX.Navigation.Common.dll.so => 280
	i64 u0x407ac43dee26bd5a, ; 270: lib_Azure.Identity.dll.so => 177
	i64 u0x40c6d9cbfdb8b9f7, ; 271: SkiaSharp.Views.Maui.Core.dll => 220
	i64 u0x40c98b6bd77346d4, ; 272: Microsoft.VisualBasic.dll => 5
	i64 u0x41833cf766d27d96, ; 273: mscorlib => 169
	i64 u0x41885d4d9eaed7f7, ; 274: CriticalInsight.Data => 352
	i64 u0x41cab042be111c34, ; 275: lib_Xamarin.AndroidX.AppCompat.AppCompatResources.dll.so => 239
	i64 u0x423a9ecc4d905a88, ; 276: lib_System.Resources.ResourceManager.dll.so => 102
	i64 u0x423bf51ae7def810, ; 277: System.Xml.XPath => 163
	i64 u0x42462ff15ddba223, ; 278: System.Resources.Reader.dll => 101
	i64 u0x4291015ff4e5ef71, ; 279: Xamarin.AndroidX.Core.ViewTree.dll => 253
	i64 u0x42a31b86e6ccc3f0, ; 280: System.Diagnostics.Contracts => 27
	i64 u0x430e95b891249788, ; 281: lib_System.Reflection.Emit.dll.so => 95
	i64 u0x43375950ec7c1b6a, ; 282: netstandard.dll => 170
	i64 u0x434c4e1d9284cdae, ; 283: Mono.Android.dll => 174
	i64 u0x43505013578652a0, ; 284: lib_Xamarin.AndroidX.Activity.Ktx.dll.so => 234
	i64 u0x437d06c381ed575a, ; 285: lib_Microsoft.VisualBasic.dll.so => 5
	i64 u0x43950f84de7cc79a, ; 286: pl/Microsoft.Maui.Controls.resources.dll => 338
	i64 u0x43e8ca5bc927ff37, ; 287: lib_Xamarin.AndroidX.Emoji2.ViewsHelper.dll.so => 260
	i64 u0x448bd33429269b19, ; 288: Microsoft.CSharp => 3
	i64 u0x4499fa3c8e494654, ; 289: lib_System.Runtime.Serialization.Primitives.dll.so => 116
	i64 u0x4515080865a951a5, ; 290: Xamarin.Kotlin.StdLib.dll => 310
	i64 u0x4545802489b736b9, ; 291: Xamarin.AndroidX.Fragment.Ktx => 263
	i64 u0x454b4d1e66bb783c, ; 292: Xamarin.AndroidX.Lifecycle.Process => 270
	i64 u0x45c40276a42e283e, ; 293: System.Diagnostics.TraceSource => 35
	i64 u0x45d443f2a29adc37, ; 294: System.AppContext.dll => 8
	i64 u0x46a4213bc97fe5ae, ; 295: lib-ru-Microsoft.Maui.Controls.resources.dll.so => 342
	i64 u0x47358bd471172e1d, ; 296: lib_System.Xml.Linq.dll.so => 158
	i64 u0x4787a936949fcac2, ; 297: System.Memory.Data.dll => 227
	i64 u0x47daf4e1afbada10, ; 298: pt/Microsoft.Maui.Controls.resources => 340
	i64 u0x480c0a47dd42dd81, ; 299: lib_System.IO.MemoryMappedFiles.dll.so => 55
	i64 u0x49e952f19a4e2022, ; 300: System.ObjectModel => 87
	i64 u0x49f9e6948a8131e4, ; 301: lib_Xamarin.AndroidX.VersionedParcelable.dll.so => 297
	i64 u0x4a5667b2462a664b, ; 302: lib_Xamarin.AndroidX.Navigation.UI.dll.so => 283
	i64 u0x4a7a18981dbd56bc, ; 303: System.IO.Compression.FileSystem.dll => 46
	i64 u0x4aa5c60350917c06, ; 304: lib_Xamarin.AndroidX.Lifecycle.LiveData.Core.Ktx.dll.so => 269
	i64 u0x4ac3e9e829632e5b, ; 305: System.Numerics.Tensors.dll => 228
	i64 u0x4b07a0ed0ab33ff4, ; 306: System.Runtime.Extensions.dll => 106
	i64 u0x4b2a3b6d8db9557a, ; 307: es/IdeaBranch.App.resources => 0
	i64 u0x4b576d47ac054f3c, ; 308: System.IO.FileSystem.AccessControl => 49
	i64 u0x4b7b6532ded934b7, ; 309: System.Text.Json => 140
	i64 u0x4b8f8ea3c2df6bb0, ; 310: System.ClientModel => 226
	i64 u0x4bf547f87e5016a8, ; 311: lib_SkiaSharp.Views.Android.dll.so => 218
	i64 u0x4c7755cf07ad2d5f, ; 312: System.Net.Http.Json.dll => 66
	i64 u0x4cc5f15266470798, ; 313: lib_Xamarin.AndroidX.Loader.dll.so => 279
	i64 u0x4cf6f67dc77aacd2, ; 314: System.Net.NetworkInformation.dll => 71
	i64 u0x4d3183dd245425d4, ; 315: System.Net.WebSockets.Client.dll => 82
	i64 u0x4d479f968a05e504, ; 316: System.Linq.Expressions.dll => 61
	i64 u0x4d55a010ffc4faff, ; 317: System.Private.Xml => 91
	i64 u0x4d5cbe77561c5b2e, ; 318: System.Web.dll => 156
	i64 u0x4d6001db23f8cd87, ; 319: lib_System.ClientModel.dll.so => 226
	i64 u0x4d77512dbd86ee4c, ; 320: lib_Xamarin.AndroidX.Arch.Core.Common.dll.so => 240
	i64 u0x4d7793536e79c309, ; 321: System.ServiceProcess => 135
	i64 u0x4d95fccc1f67c7ca, ; 322: System.Runtime.Loader.dll => 112
	i64 u0x4dcf44c3c9b076a2, ; 323: it/Microsoft.Maui.Controls.resources.dll => 332
	i64 u0x4dd9247f1d2c3235, ; 324: Xamarin.AndroidX.Loader.dll => 279
	i64 u0x4e2aeee78e2c4a87, ; 325: Xamarin.AndroidX.ProfileInstaller.ProfileInstaller => 284
	i64 u0x4e32f00cb0937401, ; 326: Mono.Android.Runtime => 173
	i64 u0x4e5eea4668ac2b18, ; 327: System.Text.Encoding.CodePages => 136
	i64 u0x4ebd0c4b82c5eefc, ; 328: lib_System.Threading.Channels.dll.so => 142
	i64 u0x4ee8eaa9c9c1151a, ; 329: System.Globalization.Calendars => 42
	i64 u0x4f21ee6ef9eb527e, ; 330: ca/Microsoft.Maui.Controls.resources => 319
	i64 u0x4fd5f3ee53d0a4f0, ; 331: SQLitePCLRaw.lib.e_sqlite3.android => 224
	i64 u0x4fdc964ec1888e25, ; 332: lib_Microsoft.Extensions.Configuration.Binder.dll.so => 192
	i64 u0x5037f0be3c28c7a3, ; 333: lib_Microsoft.Maui.Controls.dll.so => 208
	i64 u0x504bf022d1e821a4, ; 334: Microsoft.Agents.AI.Abstractions => 182
	i64 u0x50c3a29b21050d45, ; 335: System.Linq.Parallel.dll => 62
	i64 u0x5116b21580ae6eb0, ; 336: Microsoft.Extensions.Configuration.Binder.dll => 192
	i64 u0x5131bbe80989093f, ; 337: Xamarin.AndroidX.Lifecycle.ViewModel.Android.dll => 276
	i64 u0x516324a5050a7e3c, ; 338: System.Net.WebProxy => 81
	i64 u0x516d6f0b21a303de, ; 339: lib_System.Diagnostics.Contracts.dll.so => 27
	i64 u0x51bb8a2afe774e32, ; 340: System.Drawing => 38
	i64 u0x5247c5c32a4140f0, ; 341: System.Resources.Reader => 101
	i64 u0x526bb15e3c386364, ; 342: Xamarin.AndroidX.Lifecycle.Runtime.Ktx.dll => 273
	i64 u0x526ce79eb8e90527, ; 343: lib_System.Net.Primitives.dll.so => 73
	i64 u0x52829f00b4467c38, ; 344: lib_System.Data.Common.dll.so => 24
	i64 u0x529ffe06f39ab8db, ; 345: Xamarin.AndroidX.Core => 251
	i64 u0x52ff996554dbf352, ; 346: Microsoft.Maui.Graphics => 212
	i64 u0x535f7e40e8fef8af, ; 347: lib-sk-Microsoft.Maui.Controls.resources.dll.so => 343
	i64 u0x53978aac584c666e, ; 348: lib_System.Security.Cryptography.Cng.dll.so => 123
	i64 u0x53a96d5c86c9e194, ; 349: System.Net.NetworkInformation => 71
	i64 u0x53be1038a61e8d44, ; 350: System.Runtime.InteropServices.RuntimeInformation.dll => 109
	i64 u0x53c3014b9437e684, ; 351: lib-zh-HK-Microsoft.Maui.Controls.resources.dll.so => 349
	i64 u0x5435e6f049e9bc37, ; 352: System.Security.Claims.dll => 121
	i64 u0x54580f8df4389a82, ; 353: Microsoft.Agents.AI.OpenAI.dll => 183
	i64 u0x54795225dd1587af, ; 354: lib_System.Runtime.dll.so => 119
	i64 u0x547a34f14e5f6210, ; 355: Xamarin.AndroidX.Lifecycle.Common.dll => 265
	i64 u0x556e8b63b660ab8b, ; 356: Xamarin.AndroidX.Lifecycle.Common.Jvm.dll => 266
	i64 u0x5588627c9a108ec9, ; 357: System.Collections.Specialized => 13
	i64 u0x55a898e4f42e3fae, ; 358: Microsoft.VisualBasic.Core.dll => 4
	i64 u0x55fa0c610fe93bb1, ; 359: lib_System.Security.Cryptography.OpenSsl.dll.so => 126
	i64 u0x561449e1215a61e4, ; 360: lib_SkiaSharp.Views.Maui.Core.dll.so => 220
	i64 u0x56442b99bc64bb47, ; 361: System.Runtime.Serialization.Xml.dll => 117
	i64 u0x564aee2d141e20b4, ; 362: Microsoft.Extensions.AI.Abstractions => 187
	i64 u0x56a8b26e1aeae27b, ; 363: System.Threading.Tasks.Dataflow => 144
	i64 u0x56f76b6edb837f8b, ; 364: Polly => 214
	i64 u0x56f932d61e93c07f, ; 365: System.Globalization.Extensions => 43
	i64 u0x571c5cfbec5ae8e2, ; 366: System.Private.Uri => 89
	i64 u0x576499c9f52fea31, ; 367: Xamarin.AndroidX.Annotation => 235
	i64 u0x578cd35c91d7b347, ; 368: lib_SQLitePCLRaw.core.dll.so => 223
	i64 u0x579a06fed6eec900, ; 369: System.Private.CoreLib.dll => 175
	i64 u0x57c542c14049b66d, ; 370: System.Diagnostics.DiagnosticSource => 29
	i64 u0x581a8bd5cfda563e, ; 371: System.Threading.Timer => 150
	i64 u0x584ac38e21d2fde1, ; 372: Microsoft.Extensions.Configuration.Binder => 192
	i64 u0x58601b2dda4a27b9, ; 373: lib-ja-Microsoft.Maui.Controls.resources.dll.so => 333
	i64 u0x58688d9af496b168, ; 374: Microsoft.Extensions.DependencyInjection.dll => 193
	i64 u0x588c167a79db6bfb, ; 375: lib_Xamarin.Google.ErrorProne.Annotations.dll.so => 305
	i64 u0x5906028ae5151104, ; 376: Xamarin.AndroidX.Activity.Ktx => 234
	i64 u0x593995d9501261a6, ; 377: IdeaBranch.Presentation => 355
	i64 u0x595a356d23e8da9a, ; 378: lib_Microsoft.CSharp.dll.so => 3
	i64 u0x59f9e60b9475085f, ; 379: lib_Xamarin.AndroidX.Annotation.Experimental.dll.so => 236
	i64 u0x5a70033ca9d003cb, ; 380: lib_System.Memory.Data.dll.so => 227
	i64 u0x5a745f5101a75527, ; 381: lib_System.IO.Compression.FileSystem.dll.so => 46
	i64 u0x5a89a886ae30258d, ; 382: lib_Xamarin.AndroidX.CoordinatorLayout.dll.so => 250
	i64 u0x5a8f6699f4a1caa9, ; 383: lib_System.Threading.dll.so => 151
	i64 u0x5ae9cd33b15841bf, ; 384: System.ComponentModel => 20
	i64 u0x5b54391bdc6fcfe6, ; 385: System.Private.DataContractSerialization => 88
	i64 u0x5b5ba1327561f926, ; 386: lib_SkiaSharp.Views.Maui.Controls.dll.so => 219
	i64 u0x5b5f0e240a06a2a2, ; 387: da/Microsoft.Maui.Controls.resources.dll => 321
	i64 u0x5b8109e8e14c5e3e, ; 388: System.Globalization.Extensions.dll => 43
	i64 u0x5bddd04d72a9e350, ; 389: Xamarin.AndroidX.Lifecycle.LiveData.Core.Ktx => 269
	i64 u0x5bdf16b09da116ab, ; 390: Xamarin.AndroidX.Collection => 244
	i64 u0x5c019d5266093159, ; 391: lib_Xamarin.AndroidX.Lifecycle.Runtime.Ktx.Android.dll.so => 274
	i64 u0x5c30a4a35f9cc8c4, ; 392: lib_System.Reflection.Extensions.dll.so => 96
	i64 u0x5c393624b8176517, ; 393: lib_Microsoft.Extensions.Logging.dll.so => 199
	i64 u0x5c53c29f5073b0c9, ; 394: System.Diagnostics.FileVersionInfo => 30
	i64 u0x5c87463c575c7616, ; 395: lib_System.Globalization.Extensions.dll.so => 43
	i64 u0x5d0a4a29b02d9d3c, ; 396: System.Net.WebHeaderCollection.dll => 80
	i64 u0x5d40c9b15181641f, ; 397: lib_Xamarin.AndroidX.Emoji2.dll.so => 259
	i64 u0x5d6ca10d35e9485b, ; 398: lib_Xamarin.AndroidX.Concurrent.Futures.dll.so => 247
	i64 u0x5d7ec76c1c703055, ; 399: System.Threading.Tasks.Parallel => 146
	i64 u0x5db0cbbd1028510e, ; 400: lib_System.Runtime.InteropServices.dll.so => 110
	i64 u0x5db30905d3e5013b, ; 401: Xamarin.AndroidX.Collection.Jvm.dll => 245
	i64 u0x5e467bc8f09ad026, ; 402: System.Collections.Specialized.dll => 13
	i64 u0x5e5173b3208d97e7, ; 403: System.Runtime.Handles.dll => 107
	i64 u0x5ea92fdb19ec8c4c, ; 404: System.Text.Encodings.Web.dll => 139
	i64 u0x5eb8046dd40e9ac3, ; 405: System.ComponentModel.Primitives => 18
	i64 u0x5ec272d219c9aba4, ; 406: System.Security.Cryptography.Csp.dll => 124
	i64 u0x5eee1376d94c7f5e, ; 407: System.Net.HttpListener.dll => 68
	i64 u0x5f2225e69bf082b9, ; 408: OpenAI.dll => 213
	i64 u0x5f36ccf5c6a57e24, ; 409: System.Xml.ReaderWriter.dll => 159
	i64 u0x5f4294b9b63cb842, ; 410: System.Data.Common => 24
	i64 u0x5f7399e166075632, ; 411: lib_SQLitePCLRaw.lib.e_sqlite3.android.dll.so => 224
	i64 u0x5f9a2d823f664957, ; 412: lib-el-Microsoft.Maui.Controls.resources.dll.so => 323
	i64 u0x5fa6da9c3cd8142a, ; 413: lib_Xamarin.KotlinX.Serialization.Core.dll.so => 316
	i64 u0x5fac98e0b37a5b9d, ; 414: System.Runtime.CompilerServices.Unsafe.dll => 104
	i64 u0x609f4b7b63d802d4, ; 415: lib_Microsoft.Extensions.DependencyInjection.dll.so => 193
	i64 u0x60cd4e33d7e60134, ; 416: Xamarin.KotlinX.Coroutines.Core.Jvm => 315
	i64 u0x60f62d786afcf130, ; 417: System.Memory => 65
	i64 u0x61bb78c89f867353, ; 418: System.IO => 60
	i64 u0x61be8d1299194243, ; 419: Microsoft.Maui.Controls.Xaml => 209
	i64 u0x61d2cba29557038f, ; 420: de/Microsoft.Maui.Controls.resources => 322
	i64 u0x61d88f399afb2f45, ; 421: lib_System.Runtime.Loader.dll.so => 112
	i64 u0x622eef6f9e59068d, ; 422: System.Private.CoreLib => 175
	i64 u0x63b6c629a2328573, ; 423: CriticalInsight.Data.dll => 352
	i64 u0x63d5e3aa4ef9b931, ; 424: Xamarin.KotlinX.Coroutines.Android.dll => 313
	i64 u0x63f1f6883c1e23c2, ; 425: lib_System.Collections.Immutable.dll.so => 11
	i64 u0x6400f68068c1e9f1, ; 426: Xamarin.Google.Android.Material.dll => 302
	i64 u0x640e3b14dbd325c2, ; 427: System.Security.Cryptography.Algorithms.dll => 122
	i64 u0x64587004560099b9, ; 428: System.Reflection => 100
	i64 u0x64b1529a438a3c45, ; 429: lib_System.Runtime.Handles.dll.so => 107
	i64 u0x6565fba2cd8f235b, ; 430: Xamarin.AndroidX.Lifecycle.ViewModel.Ktx => 277
	i64 u0x658f524e4aba7dad, ; 431: CommunityToolkit.Maui.dll => 178
	i64 u0x65ecac39144dd3cc, ; 432: Microsoft.Maui.Controls.dll => 208
	i64 u0x65ece51227bfa724, ; 433: lib_System.Runtime.Numerics.dll.so => 113
	i64 u0x661722438787b57f, ; 434: Xamarin.AndroidX.Annotation.Jvm.dll => 237
	i64 u0x6679b2337ee6b22a, ; 435: lib_System.IO.FileSystem.Primitives.dll.so => 51
	i64 u0x6692e924eade1b29, ; 436: lib_System.Console.dll.so => 22
	i64 u0x66a4e5c6a3fb0bae, ; 437: lib_Xamarin.AndroidX.Lifecycle.ViewModel.Android.dll.so => 276
	i64 u0x66d13304ce1a3efa, ; 438: Xamarin.AndroidX.CursorAdapter => 254
	i64 u0x674303f65d8fad6f, ; 439: lib_System.Net.Quic.dll.so => 74
	i64 u0x6756ca4cad62e9d6, ; 440: lib_Xamarin.AndroidX.ConstraintLayout.Core.dll.so => 249
	i64 u0x67c0802770244408, ; 441: System.Windows.dll => 157
	i64 u0x68100b69286e27cd, ; 442: lib_System.Formats.Tar.dll.so => 41
	i64 u0x68558ec653afa616, ; 443: lib-da-Microsoft.Maui.Controls.resources.dll.so => 321
	i64 u0x6872ec7a2e36b1ac, ; 444: System.Drawing.Primitives.dll => 37
	i64 u0x68bb2c417aa9b61c, ; 445: Xamarin.KotlinX.AtomicFU.dll => 311
	i64 u0x68fbbbe2eb455198, ; 446: System.Formats.Asn1 => 40
	i64 u0x69063fc0ba8e6bdd, ; 447: he/Microsoft.Maui.Controls.resources.dll => 327
	i64 u0x699dffb2427a2d71, ; 448: SQLitePCLRaw.lib.e_sqlite3.android.dll => 224
	i64 u0x69a3e26c76f6eec4, ; 449: Xamarin.AndroidX.Window.Extensions.Core.Core.dll => 301
	i64 u0x6a4d7577b2317255, ; 450: System.Runtime.InteropServices.dll => 110
	i64 u0x6a606adbec16099d, ; 451: IdeaBranch.Infrastructure => 354
	i64 u0x6ace3b74b15ee4a4, ; 452: nb/Microsoft.Maui.Controls.resources => 336
	i64 u0x6ad70b22a26ef379, ; 453: lib-fr-IdeaBranch.App.resources.dll.so => 1
	i64 u0x6afcedb171067e2b, ; 454: System.Core.dll => 23
	i64 u0x6bef98e124147c24, ; 455: Xamarin.Jetbrains.Annotations => 308
	i64 u0x6cc16dff297ba5ce, ; 456: IdeaBranch.App => 2
	i64 u0x6ce874bff138ce2b, ; 457: Xamarin.AndroidX.Lifecycle.ViewModel.dll => 275
	i64 u0x6d12bfaa99c72b1f, ; 458: lib_Microsoft.Maui.Graphics.dll.so => 212
	i64 u0x6d70755158ca866e, ; 459: lib_System.ComponentModel.EventBasedAsync.dll.so => 17
	i64 u0x6d79993361e10ef2, ; 460: Microsoft.Extensions.Primitives => 204
	i64 u0x6d7eeca99577fc8b, ; 461: lib_System.Net.WebProxy.dll.so => 81
	i64 u0x6d8515b19946b6a2, ; 462: System.Net.WebProxy.dll => 81
	i64 u0x6d86d56b84c8eb71, ; 463: lib_Xamarin.AndroidX.CursorAdapter.dll.so => 254
	i64 u0x6d9bea6b3e895cf7, ; 464: Microsoft.Extensions.Primitives.dll => 204
	i64 u0x6e25a02c3833319a, ; 465: lib_Xamarin.AndroidX.Navigation.Fragment.dll.so => 281
	i64 u0x6e79c6bd8627412a, ; 466: Xamarin.AndroidX.SavedState.SavedState.Ktx => 288
	i64 u0x6e838d9a2a6f6c9e, ; 467: lib_System.ValueTuple.dll.so => 154
	i64 u0x6e9965ce1095e60a, ; 468: lib_System.Core.dll.so => 23
	i64 u0x6fd2265da78b93a4, ; 469: lib_Microsoft.Maui.dll.so => 210
	i64 u0x6fdfc7de82c33008, ; 470: cs/Microsoft.Maui.Controls.resources => 320
	i64 u0x6fe1b892f5da8856, ; 471: lib_Microsoft.Extensions.AI.Abstractions.dll.so => 187
	i64 u0x6ffc4967cc47ba57, ; 472: System.IO.FileSystem.Watcher.dll => 52
	i64 u0x701cd46a1c25a5fe, ; 473: System.IO.FileSystem.dll => 53
	i64 u0x70e99f48c05cb921, ; 474: tr/Microsoft.Maui.Controls.resources.dll => 346
	i64 u0x70fd3deda22442d2, ; 475: lib-nb-Microsoft.Maui.Controls.resources.dll.so => 336
	i64 u0x71485e7ffdb4b958, ; 476: System.Reflection.Extensions => 96
	i64 u0x7162a2fce67a945f, ; 477: lib_Xamarin.Android.Glide.Annotations.dll.so => 230
	i64 u0x717530326f808838, ; 478: lib_Microsoft.Extensions.Diagnostics.Abstractions.dll.so => 196
	i64 u0x71a495ea3761dde8, ; 479: lib-it-Microsoft.Maui.Controls.resources.dll.so => 332
	i64 u0x71ad672adbe48f35, ; 480: System.ComponentModel.Primitives.dll => 18
	i64 u0x720f102581a4a5c8, ; 481: Xamarin.AndroidX.Core.ViewTree => 253
	i64 u0x721d5d4df699cc70, ; 482: es/IdeaBranch.App.resources.dll => 0
	i64 u0x725f5a9e82a45c81, ; 483: System.Security.Cryptography.Encoding => 125
	i64 u0x72b1fb4109e08d7b, ; 484: lib-hr-Microsoft.Maui.Controls.resources.dll.so => 329
	i64 u0x72e0300099accce1, ; 485: System.Xml.XPath.XDocument => 162
	i64 u0x72e9010ffd52e5b7, ; 486: Microsoft.Extensions.AI.dll => 186
	i64 u0x730bfb248998f67a, ; 487: System.IO.Compression.ZipFile => 47
	i64 u0x732b2d67b9e5c47b, ; 488: Xamarin.Google.ErrorProne.Annotations.dll => 305
	i64 u0x734b76fdc0dc05bb, ; 489: lib_GoogleGson.dll.so => 180
	i64 u0x73a6be34e822f9d1, ; 490: lib_System.Runtime.Serialization.dll.so => 118
	i64 u0x73e4ce94e2eb6ffc, ; 491: lib_System.Memory.dll.so => 65
	i64 u0x743a1eccf080489a, ; 492: WindowsBase.dll => 168
	i64 u0x746cf89b511b4d40, ; 493: lib_Microsoft.Extensions.Diagnostics.dll.so => 195
	i64 u0x74f1d6f5ab554b1d, ; 494: Microsoft.Extensions.Http.Polly.dll => 198
	i64 u0x753210bfbb8ba36a, ; 495: lib_Microsoft.Extensions.AI.OpenAI.dll.so => 188
	i64 u0x755a91767330b3d4, ; 496: lib_Microsoft.Extensions.Configuration.dll.so => 190
	i64 u0x75c326eb821b85c4, ; 497: lib_System.ComponentModel.DataAnnotations.dll.so => 16
	i64 u0x76012e7334db86e5, ; 498: lib_Xamarin.AndroidX.SavedState.dll.so => 287
	i64 u0x76ca07b878f44da0, ; 499: System.Runtime.Numerics.dll => 113
	i64 u0x7736c8a96e51a061, ; 500: lib_Xamarin.AndroidX.Annotation.Jvm.dll.so => 237
	i64 u0x778a805e625329ef, ; 501: System.Linq.Parallel => 62
	i64 u0x779290cc2b801eb7, ; 502: Xamarin.KotlinX.AtomicFU.Jvm => 312
	i64 u0x77f8a4acc2fdc449, ; 503: System.Security.Cryptography.Cng.dll => 123
	i64 u0x780bc73597a503a9, ; 504: lib-ms-Microsoft.Maui.Controls.resources.dll.so => 335
	i64 u0x782c5d8eb99ff201, ; 505: lib_Microsoft.VisualBasic.Core.dll.so => 4
	i64 u0x783606d1e53e7a1a, ; 506: th/Microsoft.Maui.Controls.resources.dll => 345
	i64 u0x78a45e51311409b6, ; 507: Xamarin.AndroidX.Fragment.dll => 262
	i64 u0x78ed4ab8f9d800a1, ; 508: Xamarin.AndroidX.Lifecycle.ViewModel => 275
	i64 u0x7a25bdb29108c6e7, ; 509: Microsoft.Extensions.Http => 197
	i64 u0x7a39601d6f0bb831, ; 510: lib_Xamarin.KotlinX.AtomicFU.dll.so => 311
	i64 u0x7a5207a7c82d30b4, ; 511: lib_Xamarin.JSpecify.dll.so => 309
	i64 u0x7a7e7eddf79c5d26, ; 512: lib_Xamarin.AndroidX.Lifecycle.ViewModel.dll.so => 275
	i64 u0x7a9a57d43b0845fa, ; 513: System.AppContext => 8
	i64 u0x7ad0f4f1e5d08183, ; 514: Xamarin.AndroidX.Collection.dll => 244
	i64 u0x7adb8da2ac89b647, ; 515: fi/Microsoft.Maui.Controls.resources.dll => 325
	i64 u0x7b13d9eaa944ade8, ; 516: Xamarin.AndroidX.DynamicAnimation.dll => 258
	i64 u0x7b150145c0a9058c, ; 517: Microsoft.Data.Sqlite => 185
	i64 u0x7bef86a4335c4870, ; 518: System.ComponentModel.TypeConverter => 19
	i64 u0x7c0820144cd34d6a, ; 519: sk/Microsoft.Maui.Controls.resources.dll => 343
	i64 u0x7c2a0bd1e0f988fc, ; 520: lib-de-Microsoft.Maui.Controls.resources.dll.so => 322
	i64 u0x7c41d387501568ba, ; 521: System.Net.WebClient.dll => 79
	i64 u0x7c482cd79bd24b13, ; 522: lib_Xamarin.AndroidX.ConstraintLayout.dll.so => 248
	i64 u0x7cc637f941f716d0, ; 523: CommunityToolkit.Maui.Core => 179
	i64 u0x7cd2ec8eaf5241cd, ; 524: System.Security.dll => 133
	i64 u0x7cf9ae50dd350622, ; 525: Xamarin.Jetbrains.Annotations.dll => 308
	i64 u0x7d649b75d580bb42, ; 526: ms/Microsoft.Maui.Controls.resources.dll => 335
	i64 u0x7d8ee2bdc8e3aad1, ; 527: System.Numerics.Vectors => 85
	i64 u0x7dbeb139ed7ba6f7, ; 528: IdeaBranch.Domain.dll => 353
	i64 u0x7df5df8db8eaa6ac, ; 529: Microsoft.Extensions.Logging.Debug => 201
	i64 u0x7dfc3d6d9d8d7b70, ; 530: System.Collections => 14
	i64 u0x7e2e564fa2f76c65, ; 531: lib_System.Diagnostics.Tracing.dll.so => 36
	i64 u0x7e302e110e1e1346, ; 532: lib_System.Security.Claims.dll.so => 121
	i64 u0x7e4465b3f78ad8d0, ; 533: Xamarin.KotlinX.Serialization.Core.dll => 316
	i64 u0x7e571cad5915e6c3, ; 534: lib_Xamarin.AndroidX.Lifecycle.Process.dll.so => 270
	i64 u0x7e6b1ca712437d7d, ; 535: Xamarin.AndroidX.Emoji2.ViewsHelper => 260
	i64 u0x7e946809d6008ef2, ; 536: lib_System.ObjectModel.dll.so => 87
	i64 u0x7ea0272c1b4a9635, ; 537: lib_Xamarin.Android.Glide.dll.so => 229
	i64 u0x7ecc13347c8fd849, ; 538: lib_System.ComponentModel.dll.so => 20
	i64 u0x7f00ddd9b9ca5a13, ; 539: Xamarin.AndroidX.ViewPager.dll => 298
	i64 u0x7f9351cd44b1273f, ; 540: Microsoft.Extensions.Configuration.Abstractions => 191
	i64 u0x7fae0ef4dc4770fe, ; 541: Microsoft.Identity.Client => 205
	i64 u0x7fbd557c99b3ce6f, ; 542: lib_Xamarin.AndroidX.Lifecycle.LiveData.Core.dll.so => 268
	i64 u0x8076a9a44a2ca331, ; 543: System.Net.Quic => 74
	i64 u0x80da183a87731838, ; 544: System.Reflection.Metadata => 97
	i64 u0x80fa55b6d1b0be99, ; 545: SQLitePCLRaw.provider.e_sqlite3 => 225
	i64 u0x812c069d5cdecc17, ; 546: System.dll => 167
	i64 u0x81381be520a60adb, ; 547: Xamarin.AndroidX.Interpolator.dll => 264
	i64 u0x81657cec2b31e8aa, ; 548: System.Net => 84
	i64 u0x81ab745f6c0f5ce6, ; 549: zh-Hant/Microsoft.Maui.Controls.resources => 351
	i64 u0x8277f2be6b5ce05f, ; 550: Xamarin.AndroidX.AppCompat => 238
	i64 u0x828f06563b30bc50, ; 551: lib_Xamarin.AndroidX.CardView.dll.so => 243
	i64 u0x82920a8d9194a019, ; 552: Xamarin.KotlinX.AtomicFU.Jvm.dll => 312
	i64 u0x82b399cb01b531c4, ; 553: lib_System.Web.dll.so => 156
	i64 u0x82df8f5532a10c59, ; 554: lib_System.Drawing.dll.so => 38
	i64 u0x82f0b6e911d13535, ; 555: lib_System.Transactions.dll.so => 153
	i64 u0x82f6403342e12049, ; 556: uk/Microsoft.Maui.Controls.resources => 347
	i64 u0x83144699b312ad81, ; 557: SQLite-net.dll => 221
	i64 u0x83a7afd2c49adc86, ; 558: lib_Microsoft.IdentityModel.Abstractions.dll.so => 207
	i64 u0x83c14ba66c8e2b8c, ; 559: zh-Hans/Microsoft.Maui.Controls.resources => 350
	i64 u0x846ce984efea52c7, ; 560: System.Threading.Tasks.Parallel.dll => 146
	i64 u0x84ae73148a4557d2, ; 561: lib_System.IO.Pipes.dll.so => 58
	i64 u0x84b01102c12a9232, ; 562: System.Runtime.Serialization.Json.dll => 115
	i64 u0x84f9060cc4a93c8f, ; 563: lib_SkiaSharp.dll.so => 217
	i64 u0x850c5ba0b57ce8e7, ; 564: lib_Xamarin.AndroidX.Collection.dll.so => 244
	i64 u0x851d02edd334b044, ; 565: Xamarin.AndroidX.VectorDrawable => 295
	i64 u0x85c919db62150978, ; 566: Xamarin.AndroidX.Transition.dll => 294
	i64 u0x8662aaeb94fef37f, ; 567: lib_System.Dynamic.Runtime.dll.so => 39
	i64 u0x86a909228dc7657b, ; 568: lib-zh-Hant-Microsoft.Maui.Controls.resources.dll.so => 351
	i64 u0x86b3e00c36b84509, ; 569: Microsoft.Extensions.Configuration.dll => 190
	i64 u0x86b62cb077ec4fd7, ; 570: System.Runtime.Serialization.Xml => 117
	i64 u0x8706ffb12bf3f53d, ; 571: Xamarin.AndroidX.Annotation.Experimental => 236
	i64 u0x872a5b14c18d328c, ; 572: System.ComponentModel.DataAnnotations => 16
	i64 u0x872fb9615bc2dff0, ; 573: Xamarin.Android.Glide.Annotations.dll => 230
	i64 u0x87c69b87d9283884, ; 574: lib_System.Threading.Thread.dll.so => 148
	i64 u0x87f6569b25707834, ; 575: System.IO.Compression.Brotli.dll => 45
	i64 u0x8842b3a5d2d3fb36, ; 576: Microsoft.Maui.Essentials => 211
	i64 u0x88926583efe7ee86, ; 577: Xamarin.AndroidX.Activity.Ktx.dll => 234
	i64 u0x88ba6bc4f7762b03, ; 578: lib_System.Reflection.dll.so => 100
	i64 u0x88bda98e0cffb7a9, ; 579: lib_Xamarin.KotlinX.Coroutines.Core.Jvm.dll.so => 315
	i64 u0x8930322c7bd8f768, ; 580: netstandard => 170
	i64 u0x897a606c9e39c75f, ; 581: lib_System.ComponentModel.Primitives.dll.so => 18
	i64 u0x89911a22005b92b7, ; 582: System.IO.FileSystem.DriveInfo.dll => 50
	i64 u0x89c5188089ec2cd5, ; 583: lib_System.Runtime.InteropServices.RuntimeInformation.dll.so => 109
	i64 u0x8a19e3dc71b34b2c, ; 584: System.Reflection.TypeExtensions.dll => 99
	i64 u0x8a399a706fcbce4b, ; 585: Microsoft.Extensions.Caching.Abstractions => 189
	i64 u0x8ad229ea26432ee2, ; 586: Xamarin.AndroidX.Loader => 279
	i64 u0x8b4ff5d0fdd5faa1, ; 587: lib_System.Diagnostics.DiagnosticSource.dll.so => 29
	i64 u0x8b541d476eb3774c, ; 588: System.Security.Principal.Windows => 130
	i64 u0x8b8d01333a96d0b5, ; 589: System.Diagnostics.Process.dll => 31
	i64 u0x8b9ceca7acae3451, ; 590: lib-he-Microsoft.Maui.Controls.resources.dll.so => 327
	i64 u0x8c111b531caad455, ; 591: IdeaBranch.Presentation.dll => 355
	i64 u0x8c53ae18581b14f0, ; 592: Azure.Core => 176
	i64 u0x8cb8f612b633affb, ; 593: Xamarin.AndroidX.SavedState.SavedState.Ktx.dll => 288
	i64 u0x8cdfdb4ce85fb925, ; 594: lib_System.Security.Principal.Windows.dll.so => 130
	i64 u0x8cdfe7b8f4caa426, ; 595: System.IO.Compression.FileSystem => 46
	i64 u0x8d0f420977c2c1c7, ; 596: Xamarin.AndroidX.CursorAdapter.dll => 254
	i64 u0x8d52f7ea2796c531, ; 597: Xamarin.AndroidX.Emoji2.dll => 259
	i64 u0x8d7b8ab4b3310ead, ; 598: System.Threading => 151
	i64 u0x8da188285aadfe8e, ; 599: System.Collections.Concurrent => 10
	i64 u0x8e937db395a74375, ; 600: lib_Microsoft.Identity.Client.dll.so => 205
	i64 u0x8ed807bfe9858dfc, ; 601: Xamarin.AndroidX.Navigation.Common => 280
	i64 u0x8ee08b8194a30f48, ; 602: lib-hi-Microsoft.Maui.Controls.resources.dll.so => 328
	i64 u0x8ef7601039857a44, ; 603: lib-ro-Microsoft.Maui.Controls.resources.dll.so => 341
	i64 u0x8ef9414937d93a0a, ; 604: SQLitePCLRaw.core.dll => 223
	i64 u0x8f32c6f611f6ffab, ; 605: pt/Microsoft.Maui.Controls.resources.dll => 340
	i64 u0x8f44b45eb046bbd1, ; 606: System.ServiceModel.Web.dll => 134
	i64 u0x8f8829d21c8985a4, ; 607: lib-pt-BR-Microsoft.Maui.Controls.resources.dll.so => 339
	i64 u0x8fbf5b0114c6dcef, ; 608: System.Globalization.dll => 44
	i64 u0x8fcc8c2a81f3d9e7, ; 609: Xamarin.KotlinX.Serialization.Core => 316
	i64 u0x8fd27d934d7b3a55, ; 610: SQLitePCLRaw.core => 223
	i64 u0x90263f8448b8f572, ; 611: lib_System.Diagnostics.TraceSource.dll.so => 35
	i64 u0x903101b46fb73a04, ; 612: _Microsoft.Android.Resource.Designer => 356
	i64 u0x90393bd4865292f3, ; 613: lib_System.IO.Compression.dll.so => 48
	i64 u0x905e2b8e7ae91ae6, ; 614: System.Threading.Tasks.Extensions.dll => 145
	i64 u0x90634f86c5ebe2b5, ; 615: Xamarin.AndroidX.Lifecycle.ViewModel.Android => 276
	i64 u0x907b636704ad79ef, ; 616: lib_Microsoft.Maui.Controls.Xaml.dll.so => 209
	i64 u0x90e9efbfd68593e0, ; 617: lib_Xamarin.AndroidX.Lifecycle.LiveData.dll.so => 267
	i64 u0x91418dc638b29e68, ; 618: lib_Xamarin.AndroidX.CustomView.dll.so => 255
	i64 u0x914e81af790f609c, ; 619: lib-es-IdeaBranch.App.resources.dll.so => 0
	i64 u0x9157bd523cd7ed36, ; 620: lib_System.Text.Json.dll.so => 140
	i64 u0x916a9a7f37120c4b, ; 621: Microsoft.Extensions.AI.Abstractions.dll => 187
	i64 u0x9177432788c1aa10, ; 622: fr/IdeaBranch.App.resources => 1
	i64 u0x91a74f07b30d37e2, ; 623: System.Linq.dll => 64
	i64 u0x91cb86ea3b17111d, ; 624: System.ServiceModel.Web => 134
	i64 u0x91fa41a87223399f, ; 625: ca/Microsoft.Maui.Controls.resources.dll => 319
	i64 u0x92054e486c0c7ea7, ; 626: System.IO.FileSystem.DriveInfo => 50
	i64 u0x928614058c40c4cd, ; 627: lib_System.Xml.XPath.XDocument.dll.so => 162
	i64 u0x92b138fffca2b01e, ; 628: lib_Xamarin.AndroidX.Arch.Core.Runtime.dll.so => 241
	i64 u0x92dfc2bfc6c6a888, ; 629: Xamarin.AndroidX.Lifecycle.LiveData => 267
	i64 u0x932b4b1e383384d8, ; 630: lib_IdeaBranch.Presentation.dll.so => 355
	i64 u0x933da2c779423d68, ; 631: Xamarin.Android.Glide.Annotations => 230
	i64 u0x9388aad9b7ae40ce, ; 632: lib_Xamarin.AndroidX.Lifecycle.Common.dll.so => 265
	i64 u0x93cfa73ab28d6e35, ; 633: ms/Microsoft.Maui.Controls.resources => 335
	i64 u0x941c00d21e5c0679, ; 634: lib_Xamarin.AndroidX.Transition.dll.so => 294
	i64 u0x944077d8ca3c6580, ; 635: System.IO.Compression.dll => 48
	i64 u0x948cffedc8ed7960, ; 636: System.Xml => 166
	i64 u0x94c8990839c4bdb1, ; 637: lib_Xamarin.AndroidX.Interpolator.dll.so => 264
	i64 u0x967fc325e09bfa8c, ; 638: es/Microsoft.Maui.Controls.resources => 324
	i64 u0x9686161486d34b81, ; 639: lib_Xamarin.AndroidX.ExifInterface.dll.so => 261
	i64 u0x96e49b31fe33d427, ; 640: Microsoft.Identity.Client.Extensions.Msal => 206
	i64 u0x9732d8dbddea3d9a, ; 641: id/Microsoft.Maui.Controls.resources => 331
	i64 u0x978be80e5210d31b, ; 642: Microsoft.Maui.Graphics.dll => 212
	i64 u0x97b8c771ea3e4220, ; 643: System.ComponentModel.dll => 20
	i64 u0x97e144c9d3c6976e, ; 644: System.Collections.Concurrent.dll => 10
	i64 u0x984184e3c70d4419, ; 645: GoogleGson => 180
	i64 u0x9843944103683dd3, ; 646: Xamarin.AndroidX.Core.Core.Ktx => 252
	i64 u0x98d720cc4597562c, ; 647: System.Security.Cryptography.OpenSsl => 126
	i64 u0x991d510397f92d9d, ; 648: System.Linq.Expressions => 61
	i64 u0x996ceeb8a3da3d67, ; 649: System.Threading.Overlapped.dll => 143
	i64 u0x99a00ca5270c6878, ; 650: Xamarin.AndroidX.Navigation.Runtime => 282
	i64 u0x99cdc6d1f2d3a72f, ; 651: ko/Microsoft.Maui.Controls.resources.dll => 334
	i64 u0x9a01b1da98b6ee10, ; 652: Xamarin.AndroidX.Lifecycle.Runtime.dll => 271
	i64 u0x9a5ccc274fd6e6ee, ; 653: Jsr305Binding.dll => 303
	i64 u0x9ae6940b11c02876, ; 654: lib_Xamarin.AndroidX.Window.dll.so => 300
	i64 u0x9b211a749105beac, ; 655: System.Transactions.Local => 152
	i64 u0x9b8734714671022d, ; 656: System.Threading.Tasks.Dataflow.dll => 144
	i64 u0x9bc6aea27fbf034f, ; 657: lib_Xamarin.KotlinX.Coroutines.Core.dll.so => 314
	i64 u0x9bd8cc74558ad4c7, ; 658: Xamarin.KotlinX.AtomicFU => 311
	i64 u0x9c244ac7cda32d26, ; 659: System.Security.Cryptography.X509Certificates.dll => 128
	i64 u0x9c465f280cf43733, ; 660: lib_Xamarin.KotlinX.Coroutines.Android.dll.so => 313
	i64 u0x9c8f6872beab6408, ; 661: System.Xml.XPath.XDocument.dll => 162
	i64 u0x9ce01cf91101ae23, ; 662: System.Xml.XmlDocument => 164
	i64 u0x9d052eb79c53b587, ; 663: lib_Polly.dll.so => 214
	i64 u0x9d128180c81d7ce6, ; 664: Xamarin.AndroidX.CustomView.PoolingContainer => 256
	i64 u0x9d5dbcf5a48583fe, ; 665: lib_Xamarin.AndroidX.Activity.dll.so => 233
	i64 u0x9d74dee1a7725f34, ; 666: Microsoft.Extensions.Configuration.Abstractions.dll => 191
	i64 u0x9e4534b6adaf6e84, ; 667: nl/Microsoft.Maui.Controls.resources => 337
	i64 u0x9e4b95dec42769f7, ; 668: System.Diagnostics.Debug.dll => 28
	i64 u0x9e5246c682c1a5ba, ; 669: IdeaBranch.Domain => 353
	i64 u0x9eaf1efdf6f7267e, ; 670: Xamarin.AndroidX.Navigation.Common.dll => 280
	i64 u0x9ee6ffcd8a354b81, ; 671: Polly.Extensions.Http.dll => 216
	i64 u0x9ef542cf1f78c506, ; 672: Xamarin.AndroidX.Lifecycle.LiveData.Core => 268
	i64 u0x9ffbb6b1434ad2df, ; 673: Microsoft.Identity.Client.dll => 205
	i64 u0xa00832eb975f56a8, ; 674: lib_System.Net.dll.so => 84
	i64 u0xa0ad78236b7b267f, ; 675: Xamarin.AndroidX.Window => 300
	i64 u0xa0d8259f4cc284ec, ; 676: lib_System.Security.Cryptography.dll.so => 129
	i64 u0xa0e17ca50c77a225, ; 677: lib_Xamarin.Google.Crypto.Tink.Android.dll.so => 304
	i64 u0xa0ff9b3e34d92f11, ; 678: lib_System.Resources.Writer.dll.so => 103
	i64 u0xa12fbfb4da97d9f3, ; 679: System.Threading.Timer.dll => 150
	i64 u0xa1440773ee9d341e, ; 680: Xamarin.Google.Android.Material => 302
	i64 u0xa1b9d7c27f47219f, ; 681: Xamarin.AndroidX.Navigation.UI.dll => 283
	i64 u0xa2572680829d2c7c, ; 682: System.IO.Pipelines.dll => 56
	i64 u0xa26597e57ee9c7f6, ; 683: System.Xml.XmlDocument.dll => 164
	i64 u0xa2beee74530fc01c, ; 684: SkiaSharp.Views.Android => 218
	i64 u0xa308401900e5bed3, ; 685: lib_mscorlib.dll.so => 169
	i64 u0xa395572e7da6c99d, ; 686: lib_System.Security.dll.so => 133
	i64 u0xa3e683f24b43af6f, ; 687: System.Dynamic.Runtime.dll => 39
	i64 u0xa4145becdee3dc4f, ; 688: Xamarin.AndroidX.VectorDrawable.Animated => 296
	i64 u0xa46aa1eaa214539b, ; 689: ko/Microsoft.Maui.Controls.resources => 334
	i64 u0xa4a372eecb9e4df0, ; 690: Microsoft.Extensions.Diagnostics => 195
	i64 u0xa4edc8f2ceae241a, ; 691: System.Data.Common.dll => 24
	i64 u0xa5494f40f128ce6a, ; 692: System.Runtime.Serialization.Formatters.dll => 114
	i64 u0xa54b74df83dce92b, ; 693: System.Reflection.DispatchProxy => 92
	i64 u0xa5b7152421ed6d98, ; 694: lib_System.IO.FileSystem.Watcher.dll.so => 52
	i64 u0xa5c3844f17b822db, ; 695: lib_System.Linq.Parallel.dll.so => 62
	i64 u0xa5ce5c755bde8cb8, ; 696: lib_System.Security.Cryptography.Csp.dll.so => 124
	i64 u0xa5e599d1e0524750, ; 697: System.Numerics.Vectors.dll => 85
	i64 u0xa5f1ba49b85dd355, ; 698: System.Security.Cryptography.dll => 129
	i64 u0xa61975a5a37873ea, ; 699: lib_System.Xml.XmlSerializer.dll.so => 165
	i64 u0xa6593e21584384d2, ; 700: lib_Jsr305Binding.dll.so => 303
	i64 u0xa66cbee0130865f7, ; 701: lib_WindowsBase.dll.so => 168
	i64 u0xa67dbee13e1df9ca, ; 702: Xamarin.AndroidX.SavedState.dll => 287
	i64 u0xa684b098dd27b296, ; 703: lib_Xamarin.AndroidX.Security.SecurityCrypto.dll.so => 289
	i64 u0xa68a420042bb9b1f, ; 704: Xamarin.AndroidX.DrawerLayout.dll => 257
	i64 u0xa6d26156d1cacc7c, ; 705: Xamarin.Android.Glide.dll => 229
	i64 u0xa75386b5cb9595aa, ; 706: Xamarin.AndroidX.Lifecycle.Runtime.Android => 272
	i64 u0xa763fbb98df8d9fb, ; 707: lib_Microsoft.Win32.Primitives.dll.so => 6
	i64 u0xa78ce3745383236a, ; 708: Xamarin.AndroidX.Lifecycle.Common.Jvm => 266
	i64 u0xa7c31b56b4dc7b33, ; 709: hu/Microsoft.Maui.Controls.resources => 330
	i64 u0xa7eab29ed44b4e7a, ; 710: Mono.Android.Export => 172
	i64 u0xa8195217cbf017b7, ; 711: Microsoft.VisualBasic.Core => 4
	i64 u0xa859a95830f367ff, ; 712: lib_Xamarin.AndroidX.Lifecycle.ViewModel.Ktx.dll.so => 277
	i64 u0xa8b52f21e0dbe690, ; 713: System.Runtime.Serialization.dll => 118
	i64 u0xa8ee4ed7de2efaee, ; 714: Xamarin.AndroidX.Annotation.dll => 235
	i64 u0xa95590e7c57438a4, ; 715: System.Configuration => 21
	i64 u0xa964304b5631e28a, ; 716: CommunityToolkit.Maui.Core.dll => 179
	i64 u0xaa2219c8e3449ff5, ; 717: Microsoft.Extensions.Logging.Abstractions => 200
	i64 u0xaa443ac34067eeef, ; 718: System.Private.Xml.dll => 91
	i64 u0xaa52de307ef5d1dd, ; 719: System.Net.Http => 67
	i64 u0xaa9a7b0214a5cc5c, ; 720: System.Diagnostics.StackTrace.dll => 32
	i64 u0xaaaf86367285a918, ; 721: Microsoft.Extensions.DependencyInjection.Abstractions.dll => 194
	i64 u0xaaf84bb3f052a265, ; 722: el/Microsoft.Maui.Controls.resources => 323
	i64 u0xab9af77b5b67a0b8, ; 723: Xamarin.AndroidX.ConstraintLayout.Core => 249
	i64 u0xab9c1b2687d86b0b, ; 724: lib_System.Linq.Expressions.dll.so => 61
	i64 u0xac2af3fa195a15ce, ; 725: System.Runtime.Numerics => 113
	i64 u0xac5376a2a538dc10, ; 726: Xamarin.AndroidX.Lifecycle.LiveData.Core.dll => 268
	i64 u0xac5acae88f60357e, ; 727: System.Diagnostics.Tools.dll => 34
	i64 u0xac79c7e46047ad98, ; 728: System.Security.Principal.Windows.dll => 130
	i64 u0xac98d31068e24591, ; 729: System.Xml.XDocument => 161
	i64 u0xacd46e002c3ccb97, ; 730: ro/Microsoft.Maui.Controls.resources => 341
	i64 u0xacdd9e4180d56dda, ; 731: Xamarin.AndroidX.Concurrent.Futures => 247
	i64 u0xacf42eea7ef9cd12, ; 732: System.Threading.Channels => 142
	i64 u0xad89c07347f1bad6, ; 733: nl/Microsoft.Maui.Controls.resources.dll => 337
	i64 u0xadbb53caf78a79d2, ; 734: System.Web.HttpUtility => 155
	i64 u0xadc90ab061a9e6e4, ; 735: System.ComponentModel.TypeConverter.dll => 19
	i64 u0xadca1b9030b9317e, ; 736: Xamarin.AndroidX.Collection.Ktx => 246
	i64 u0xadd8eda2edf396ad, ; 737: Xamarin.Android.Glide.GifDecoder => 232
	i64 u0xadf4cf30debbeb9a, ; 738: System.Net.ServicePoint.dll => 77
	i64 u0xadf511667bef3595, ; 739: System.Net.Security => 76
	i64 u0xae0aaa94fdcfce0f, ; 740: System.ComponentModel.EventBasedAsync.dll => 17
	i64 u0xae10ec8bb9741545, ; 741: lib_IdeaBranch.Infrastructure.dll.so => 354
	i64 u0xae282bcd03739de7, ; 742: Java.Interop => 171
	i64 u0xae53579c90db1107, ; 743: System.ObjectModel.dll => 87
	i64 u0xae7ea18c61eef394, ; 744: SQLite-net => 221
	i64 u0xaec7c0c7e2ed4575, ; 745: lib_Xamarin.KotlinX.AtomicFU.Jvm.dll.so => 312
	i64 u0xaf732d0b2193b8f5, ; 746: System.Security.Cryptography.OpenSsl.dll => 126
	i64 u0xafdb94dbccd9d11c, ; 747: Xamarin.AndroidX.Lifecycle.LiveData.dll => 267
	i64 u0xafe29f45095518e7, ; 748: lib_Xamarin.AndroidX.Lifecycle.ViewModelSavedState.dll.so => 278
	i64 u0xb03ae931fb25607e, ; 749: Xamarin.AndroidX.ConstraintLayout => 248
	i64 u0xb05cc42cd94c6d9d, ; 750: lib-sv-Microsoft.Maui.Controls.resources.dll.so => 344
	i64 u0xb09f0bfcb831862c, ; 751: System.Numerics.Tensors => 228
	i64 u0xb0ac21bec8f428c5, ; 752: Xamarin.AndroidX.Lifecycle.Runtime.Ktx.Android.dll => 274
	i64 u0xb0bb43dc52ea59f9, ; 753: System.Diagnostics.Tracing.dll => 36
	i64 u0xb1dd05401aa8ee63, ; 754: System.Security.AccessControl => 120
	i64 u0xb220631954820169, ; 755: System.Text.RegularExpressions => 141
	i64 u0xb2376e1dbf8b4ed7, ; 756: System.Security.Cryptography.Csp => 124
	i64 u0xb2a1959fe95c5402, ; 757: lib_System.Runtime.InteropServices.JavaScript.dll.so => 108
	i64 u0xb2a3f67f3bf29fce, ; 758: da/Microsoft.Maui.Controls.resources => 321
	i64 u0xb3874072ee0ecf8c, ; 759: Xamarin.AndroidX.VectorDrawable.Animated.dll => 296
	i64 u0xb3f0a0fcda8d3ebc, ; 760: Xamarin.AndroidX.CardView => 243
	i64 u0xb46be1aa6d4fff93, ; 761: hi/Microsoft.Maui.Controls.resources => 328
	i64 u0xb477491be13109d8, ; 762: ar/Microsoft.Maui.Controls.resources => 318
	i64 u0xb4bd7015ecee9d86, ; 763: System.IO.Pipelines => 56
	i64 u0xb4c53d9749c5f226, ; 764: lib_System.IO.FileSystem.AccessControl.dll.so => 49
	i64 u0xb4ff710863453fda, ; 765: System.Diagnostics.FileVersionInfo.dll => 30
	i64 u0xb5c38bf497a4cfe2, ; 766: lib_System.Threading.Tasks.dll.so => 147
	i64 u0xb5c7fcdafbc67ee4, ; 767: Microsoft.Extensions.Logging.Abstractions.dll => 200
	i64 u0xb5ea31d5244c6626, ; 768: System.Threading.ThreadPool.dll => 149
	i64 u0xb7212c4683a94afe, ; 769: System.Drawing.Primitives => 37
	i64 u0xb7b7753d1f319409, ; 770: sv/Microsoft.Maui.Controls.resources => 344
	i64 u0xb81a2c6e0aee50fe, ; 771: lib_System.Private.CoreLib.dll.so => 175
	i64 u0xb872c26142d22aa9, ; 772: Microsoft.Extensions.Http.dll => 197
	i64 u0xb8b0a9b3dfbc5cb7, ; 773: Xamarin.AndroidX.Window.Extensions.Core.Core => 301
	i64 u0xb8c60af47c08d4da, ; 774: System.Net.ServicePoint => 77
	i64 u0xb8d7556c2a240182, ; 775: lib_IdeaBranch.Domain.dll.so => 353
	i64 u0xb8e68d20aad91196, ; 776: lib_System.Xml.XPath.dll.so => 163
	i64 u0xb9185c33a1643eed, ; 777: Microsoft.CSharp.dll => 3
	i64 u0xb9b8001adf4ed7cc, ; 778: lib_Xamarin.AndroidX.SlidingPaneLayout.dll.so => 290
	i64 u0xb9f64d3b230def68, ; 779: lib-pt-Microsoft.Maui.Controls.resources.dll.so => 340
	i64 u0xb9fc3c8a556e3691, ; 780: ja/Microsoft.Maui.Controls.resources => 333
	i64 u0xba4670aa94a2b3c6, ; 781: lib_System.Xml.XDocument.dll.so => 161
	i64 u0xba48785529705af9, ; 782: System.Collections.dll => 14
	i64 u0xba965b8c86359996, ; 783: lib_System.Windows.dll.so => 157
	i64 u0xbb286883bc35db36, ; 784: System.Transactions.dll => 153
	i64 u0xbb65706fde942ce3, ; 785: System.Net.Sockets => 78
	i64 u0xbb8c8d165ef11460, ; 786: lib_Microsoft.Identity.Client.Extensions.Msal.dll.so => 206
	i64 u0xbba28979413cad9e, ; 787: lib_System.Runtime.CompilerServices.VisualC.dll.so => 105
	i64 u0xbbd180354b67271a, ; 788: System.Runtime.Serialization.Formatters => 114
	i64 u0xbc22a245dab70cb4, ; 789: lib_SQLitePCLRaw.provider.e_sqlite3.dll.so => 225
	i64 u0xbc260cdba33291a3, ; 790: Xamarin.AndroidX.Arch.Core.Common.dll => 240
	i64 u0xbd0e2c0d55246576, ; 791: System.Net.Http.dll => 67
	i64 u0xbd3fbd85b9e1cb29, ; 792: lib_System.Net.HttpListener.dll.so => 68
	i64 u0xbd437a2cdb333d0d, ; 793: Xamarin.AndroidX.ViewPager2 => 299
	i64 u0xbd4f572d2bd0a789, ; 794: System.IO.Compression.ZipFile.dll => 47
	i64 u0xbd5d0b88d3d647a5, ; 795: lib_Xamarin.AndroidX.Browser.dll.so => 242
	i64 u0xbd877b14d0b56392, ; 796: System.Runtime.Intrinsics.dll => 111
	i64 u0xbe65a49036345cf4, ; 797: lib_System.Buffers.dll.so => 9
	i64 u0xbee38d4a88835966, ; 798: Xamarin.AndroidX.AppCompat.AppCompatResources => 239
	i64 u0xbef9919db45b4ca7, ; 799: System.IO.Pipes.AccessControl => 57
	i64 u0xbf0fa68611139208, ; 800: lib_Xamarin.AndroidX.Annotation.dll.so => 235
	i64 u0xbfc1e1fb3095f2b3, ; 801: lib_System.Net.Http.Json.dll.so => 66
	i64 u0xc040a4ab55817f58, ; 802: ar/Microsoft.Maui.Controls.resources.dll => 318
	i64 u0xc07cadab29efeba0, ; 803: Xamarin.AndroidX.Core.Core.Ktx.dll => 252
	i64 u0xc0d928351ab5ca77, ; 804: System.Console.dll => 22
	i64 u0xc0f5a221a9383aea, ; 805: System.Runtime.Intrinsics => 111
	i64 u0xc111030af54d7191, ; 806: System.Resources.Writer => 103
	i64 u0xc12b8b3afa48329c, ; 807: lib_System.Linq.dll.so => 64
	i64 u0xc183ca0b74453aa9, ; 808: lib_System.Threading.Tasks.Dataflow.dll.so => 144
	i64 u0xc1bf5b6156754cda, ; 809: lib_Microsoft.Agents.AI.OpenAI.dll.so => 183
	i64 u0xc1ff9ae3cdb6e1e6, ; 810: Xamarin.AndroidX.Activity.dll => 233
	i64 u0xc26c064effb1dea9, ; 811: System.Buffers.dll => 9
	i64 u0xc28c50f32f81cc73, ; 812: ja/Microsoft.Maui.Controls.resources.dll => 333
	i64 u0xc2902f6cf5452577, ; 813: lib_Mono.Android.Export.dll.so => 172
	i64 u0xc2a3bca55b573141, ; 814: System.IO.FileSystem.Watcher => 52
	i64 u0xc2bcfec99f69365e, ; 815: Xamarin.AndroidX.ViewPager2.dll => 299
	i64 u0xc30b52815b58ac2c, ; 816: lib_System.Runtime.Serialization.Xml.dll.so => 117
	i64 u0xc36d7d89c652f455, ; 817: System.Threading.Overlapped => 143
	i64 u0xc396b285e59e5493, ; 818: GoogleGson.dll => 180
	i64 u0xc3c86c1e5e12f03d, ; 819: WindowsBase => 168
	i64 u0xc421b61fd853169d, ; 820: lib_System.Net.WebSockets.Client.dll.so => 82
	i64 u0xc463e077917aa21d, ; 821: System.Runtime.Serialization.Json => 115
	i64 u0xc4d3858ed4d08512, ; 822: Xamarin.AndroidX.Lifecycle.ViewModelSavedState.dll => 278
	i64 u0xc50fded0ded1418c, ; 823: lib_System.ComponentModel.TypeConverter.dll.so => 19
	i64 u0xc519125d6bc8fb11, ; 824: lib_System.Net.Requests.dll.so => 75
	i64 u0xc5293b19e4dc230e, ; 825: Xamarin.AndroidX.Navigation.Fragment => 281
	i64 u0xc5325b2fcb37446f, ; 826: lib_System.Private.Xml.dll.so => 91
	i64 u0xc535cb9a21385d9b, ; 827: lib_Xamarin.Android.Glide.DiskLruCache.dll.so => 231
	i64 u0xc5a0f4b95a699af7, ; 828: lib_System.Private.Uri.dll.so => 89
	i64 u0xc5cdcd5b6277579e, ; 829: lib_System.Security.Cryptography.Algorithms.dll.so => 122
	i64 u0xc5ec286825cb0bf4, ; 830: Xamarin.AndroidX.Tracing.Tracing => 293
	i64 u0xc6706bc8aa7fe265, ; 831: Xamarin.AndroidX.Annotation.Jvm => 237
	i64 u0xc7c01e7d7c93a110, ; 832: System.Text.Encoding.Extensions.dll => 137
	i64 u0xc7ce851898a4548e, ; 833: lib_System.Web.HttpUtility.dll.so => 155
	i64 u0xc809d4089d2556b2, ; 834: System.Runtime.InteropServices.JavaScript.dll => 108
	i64 u0xc858a28d9ee5a6c5, ; 835: lib_System.Collections.Specialized.dll.so => 13
	i64 u0xc8ac7c6bf1c2ec51, ; 836: System.Reflection.DispatchProxy.dll => 92
	i64 u0xc918f262b38264fa, ; 837: lib_Microsoft.Extensions.AI.dll.so => 186
	i64 u0xc9c62c8f354ac568, ; 838: lib_System.Diagnostics.TextWriterTraceListener.dll.so => 33
	i64 u0xc9e54b32fc19baf3, ; 839: lib_CommunityToolkit.Maui.dll.so => 178
	i64 u0xca3a723e7342c5b6, ; 840: lib-tr-Microsoft.Maui.Controls.resources.dll.so => 346
	i64 u0xca5801070d9fccfb, ; 841: System.Text.Encoding => 138
	i64 u0xcab3493c70141c2d, ; 842: pl/Microsoft.Maui.Controls.resources => 338
	i64 u0xcab69b9a31439815, ; 843: lib_Xamarin.Google.ErrorProne.TypeAnnotations.dll.so => 306
	i64 u0xcacfddc9f7c6de76, ; 844: ro/Microsoft.Maui.Controls.resources.dll => 341
	i64 u0xcadbc92899a777f0, ; 845: Xamarin.AndroidX.Startup.StartupRuntime => 291
	i64 u0xcba1cb79f45292b5, ; 846: Xamarin.Android.Glide.GifDecoder.dll => 232
	i64 u0xcbb5f80c7293e696, ; 847: lib_System.Globalization.Calendars.dll.so => 42
	i64 u0xcbd4fdd9cef4a294, ; 848: lib__Microsoft.Android.Resource.Designer.dll.so => 356
	i64 u0xcbf8c1d4c780610e, ; 849: lib_Polly.Core.dll.so => 215
	i64 u0xcc15da1e07bbd994, ; 850: Xamarin.AndroidX.SlidingPaneLayout => 290
	i64 u0xcc182c3afdc374d6, ; 851: Microsoft.Bcl.AsyncInterfaces => 184
	i64 u0xcc2876b32ef2794c, ; 852: lib_System.Text.RegularExpressions.dll.so => 141
	i64 u0xcc5c3bb714c4561e, ; 853: Xamarin.KotlinX.Coroutines.Core.Jvm.dll => 315
	i64 u0xcc76886e09b88260, ; 854: Xamarin.KotlinX.Serialization.Core.Jvm.dll => 317
	i64 u0xcc9fa2923aa1c9ef, ; 855: System.Diagnostics.Contracts.dll => 27
	i64 u0xccf25c4b634ccd3a, ; 856: zh-Hans/Microsoft.Maui.Controls.resources.dll => 350
	i64 u0xcd10a42808629144, ; 857: System.Net.Requests => 75
	i64 u0xcdca1b920e9f53ba, ; 858: Xamarin.AndroidX.Interpolator => 264
	i64 u0xcdd0c48b6937b21c, ; 859: Xamarin.AndroidX.SwipeRefreshLayout => 292
	i64 u0xceb28d385f84f441, ; 860: Azure.Core.dll => 176
	i64 u0xcf23d8093f3ceadf, ; 861: System.Diagnostics.DiagnosticSource.dll => 29
	i64 u0xcf5ff6b6b2c4c382, ; 862: System.Net.Mail.dll => 69
	i64 u0xcf8fc898f98b0d34, ; 863: System.Private.Xml.Linq => 90
	i64 u0xd04b5f59ed596e31, ; 864: System.Reflection.Metadata.dll => 97
	i64 u0xd063299fcfc0c93f, ; 865: lib_System.Runtime.Serialization.Json.dll.so => 115
	i64 u0xd0de8a113e976700, ; 866: System.Diagnostics.TextWriterTraceListener => 33
	i64 u0xd0fc33d5ae5d4cb8, ; 867: System.Runtime.Extensions => 106
	i64 u0xd1194e1d8a8de83c, ; 868: lib_Xamarin.AndroidX.Lifecycle.Common.Jvm.dll.so => 266
	i64 u0xd12beacdfc14f696, ; 869: System.Dynamic.Runtime => 39
	i64 u0xd16fd7fb9bbcd43e, ; 870: Microsoft.Extensions.Diagnostics.Abstractions => 196
	i64 u0xd198e7ce1b6a8344, ; 871: System.Net.Quic.dll => 74
	i64 u0xd3144156a3727ebe, ; 872: Xamarin.Google.Guava.ListenableFuture => 307
	i64 u0xd322c86d63d58c34, ; 873: lib_System.Numerics.Tensors.dll.so => 228
	i64 u0xd333d0af9e423810, ; 874: System.Runtime.InteropServices => 110
	i64 u0xd33a415cb4278969, ; 875: System.Security.Cryptography.Encoding.dll => 125
	i64 u0xd3426d966bb704f5, ; 876: Xamarin.AndroidX.AppCompat.AppCompatResources.dll => 239
	i64 u0xd3651b6fc3125825, ; 877: System.Private.Uri.dll => 89
	i64 u0xd373685349b1fe8b, ; 878: Microsoft.Extensions.Logging.dll => 199
	i64 u0xd3801faafafb7698, ; 879: System.Private.DataContractSerialization.dll => 88
	i64 u0xd3e4c8d6a2d5d470, ; 880: it/Microsoft.Maui.Controls.resources => 332
	i64 u0xd3edcc1f25459a50, ; 881: System.Reflection.Emit => 95
	i64 u0xd4645626dffec99d, ; 882: lib_Microsoft.Extensions.DependencyInjection.Abstractions.dll.so => 194
	i64 u0xd4fa0abb79079ea9, ; 883: System.Security.Principal.dll => 131
	i64 u0xd5507e11a2b2839f, ; 884: Xamarin.AndroidX.Lifecycle.ViewModelSavedState => 278
	i64 u0xd5c36a49d8a1735f, ; 885: lib_IdeaBranch.App.dll.so => 2
	i64 u0xd5d04bef8478ea19, ; 886: Xamarin.AndroidX.Tracing.Tracing.dll => 293
	i64 u0xd6037f55b1ff6c71, ; 887: fr/IdeaBranch.App.resources.dll => 1
	i64 u0xd60815f26a12e140, ; 888: Microsoft.Extensions.Logging.Debug.dll => 201
	i64 u0xd6694f8359737e4e, ; 889: Xamarin.AndroidX.SavedState => 287
	i64 u0xd6949e129339eae5, ; 890: lib_Xamarin.AndroidX.Core.Core.Ktx.dll.so => 252
	i64 u0xd6d21782156bc35b, ; 891: Xamarin.AndroidX.SwipeRefreshLayout.dll => 292
	i64 u0xd6de019f6af72435, ; 892: Xamarin.AndroidX.ConstraintLayout.Core.dll => 249
	i64 u0xd6f697a581fc6fe3, ; 893: Xamarin.Google.ErrorProne.TypeAnnotations.dll => 306
	i64 u0xd70956d1e6deefb9, ; 894: Jsr305Binding => 303
	i64 u0xd72329819cbbbc44, ; 895: lib_Microsoft.Extensions.Configuration.Abstractions.dll.so => 191
	i64 u0xd72c760af136e863, ; 896: System.Xml.XmlSerializer.dll => 165
	i64 u0xd753f071e44c2a03, ; 897: lib_System.Security.SecureString.dll.so => 132
	i64 u0xd7b3764ada9d341d, ; 898: lib_Microsoft.Extensions.Logging.Abstractions.dll.so => 200
	i64 u0xd7f0088bc5ad71f2, ; 899: Xamarin.AndroidX.VersionedParcelable => 297
	i64 u0xd8fb25e28ae30a12, ; 900: Xamarin.AndroidX.ProfileInstaller.ProfileInstaller.dll => 284
	i64 u0xda1dfa4c534a9251, ; 901: Microsoft.Extensions.DependencyInjection => 193
	i64 u0xdad05a11827959a3, ; 902: System.Collections.NonGeneric.dll => 12
	i64 u0xdaefdfe71aa53cf9, ; 903: System.IO.FileSystem.Primitives => 51
	i64 u0xdb5383ab5865c007, ; 904: lib-vi-Microsoft.Maui.Controls.resources.dll.so => 348
	i64 u0xdb58816721c02a59, ; 905: lib_System.Reflection.Emit.ILGeneration.dll.so => 93
	i64 u0xdb8f858873e2186b, ; 906: SkiaSharp.Views.Maui.Controls => 219
	i64 u0xdbeda89f832aa805, ; 907: vi/Microsoft.Maui.Controls.resources.dll => 348
	i64 u0xdbf2a779fbc3ac31, ; 908: System.Transactions.Local.dll => 152
	i64 u0xdbf9607a441b4505, ; 909: System.Linq => 64
	i64 u0xdbfc90157a0de9b0, ; 910: lib_System.Text.Encoding.dll.so => 138
	i64 u0xdc75032002d1a212, ; 911: lib_System.Transactions.Local.dll.so => 152
	i64 u0xdca8be7403f92d4f, ; 912: lib_System.Linq.Queryable.dll.so => 63
	i64 u0xdce2c53525640bf3, ; 913: Microsoft.Extensions.Logging => 199
	i64 u0xdd2b722d78ef5f43, ; 914: System.Runtime.dll => 119
	i64 u0xdd67031857c72f96, ; 915: lib_System.Text.Encodings.Web.dll.so => 139
	i64 u0xdd70765ad6162057, ; 916: Xamarin.JSpecify => 309
	i64 u0xdd92e229ad292030, ; 917: System.Numerics.dll => 86
	i64 u0xdde30e6b77aa6f6c, ; 918: lib-zh-Hans-Microsoft.Maui.Controls.resources.dll.so => 350
	i64 u0xddf3ef21cad250f2, ; 919: lib_OpenAI.dll.so => 213
	i64 u0xde110ae80fa7c2e2, ; 920: System.Xml.XDocument.dll => 161
	i64 u0xde4726fcdf63a198, ; 921: Xamarin.AndroidX.Transition => 294
	i64 u0xde572c2b2fb32f93, ; 922: lib_System.Threading.Tasks.Extensions.dll.so => 145
	i64 u0xde8769ebda7d8647, ; 923: hr/Microsoft.Maui.Controls.resources.dll => 329
	i64 u0xdee075f3477ef6be, ; 924: Xamarin.AndroidX.ExifInterface.dll => 261
	i64 u0xdf4b773de8fb1540, ; 925: System.Net.dll => 84
	i64 u0xdfa254ebb4346068, ; 926: System.Net.Ping => 72
	i64 u0xe0142572c095a480, ; 927: Xamarin.AndroidX.AppCompat.dll => 238
	i64 u0xe021eaa401792a05, ; 928: System.Text.Encoding.dll => 138
	i64 u0xe02f89350ec78051, ; 929: Xamarin.AndroidX.CoordinatorLayout.dll => 250
	i64 u0xe0496b9d65ef5474, ; 930: Xamarin.Android.Glide.DiskLruCache.dll => 231
	i64 u0xe10b760bb1462e7a, ; 931: lib_System.Security.Cryptography.Primitives.dll.so => 127
	i64 u0xe145b13d283d395a, ; 932: lib_Microsoft.Agents.AI.dll.so => 181
	i64 u0xe15ed77ed6b88cf8, ; 933: lib_CriticalInsight.Data.dll.so => 352
	i64 u0xe192a588d4410686, ; 934: lib_System.IO.Pipelines.dll.so => 56
	i64 u0xe1a08bd3fa539e0d, ; 935: System.Runtime.Loader => 112
	i64 u0xe1a77eb8831f7741, ; 936: System.Security.SecureString.dll => 132
	i64 u0xe1b52f9f816c70ef, ; 937: System.Private.Xml.Linq.dll => 90
	i64 u0xe1e199c8ab02e356, ; 938: System.Data.DataSetExtensions.dll => 25
	i64 u0xe1ecfdb7fff86067, ; 939: System.Net.Security.dll => 76
	i64 u0xe2252a80fe853de4, ; 940: lib_System.Security.Principal.dll.so => 131
	i64 u0xe22fa4c9c645db62, ; 941: System.Diagnostics.TextWriterTraceListener.dll => 33
	i64 u0xe2420585aeceb728, ; 942: System.Net.Requests.dll => 75
	i64 u0xe26692647e6bcb62, ; 943: Xamarin.AndroidX.Lifecycle.Runtime.Ktx => 273
	i64 u0xe29b73bc11392966, ; 944: lib-id-Microsoft.Maui.Controls.resources.dll.so => 331
	i64 u0xe2ad448dee50fbdf, ; 945: System.Xml.Serialization => 160
	i64 u0xe2d920f978f5d85c, ; 946: System.Data.DataSetExtensions => 25
	i64 u0xe2e426c7714fa0bc, ; 947: Microsoft.Win32.Primitives.dll => 6
	i64 u0xe332bacb3eb4a806, ; 948: Mono.Android.Export.dll => 172
	i64 u0xe3811d68d4fe8463, ; 949: pt-BR/Microsoft.Maui.Controls.resources.dll => 339
	i64 u0xe3a586956771a0ed, ; 950: lib_SQLite-net.dll.so => 221
	i64 u0xe3b7cbae5ad66c75, ; 951: lib_System.Security.Cryptography.Encoding.dll.so => 125
	i64 u0xe4292b48f3224d5b, ; 952: lib_Xamarin.AndroidX.Core.ViewTree.dll.so => 253
	i64 u0xe494f7ced4ecd10a, ; 953: hu/Microsoft.Maui.Controls.resources.dll => 330
	i64 u0xe4a9b1e40d1e8917, ; 954: lib-fi-Microsoft.Maui.Controls.resources.dll.so => 325
	i64 u0xe4f74a0b5bf9703f, ; 955: System.Runtime.Serialization.Primitives => 116
	i64 u0xe5434e8a119ceb69, ; 956: lib_Mono.Android.dll.so => 174
	i64 u0xe55703b9ce5c038a, ; 957: System.Diagnostics.Tools => 34
	i64 u0xe57013c8afc270b5, ; 958: Microsoft.VisualBasic => 5
	i64 u0xe5876071b3482131, ; 959: Polly.Extensions.Http => 216
	i64 u0xe62913cc36bc07ec, ; 960: System.Xml.dll => 166
	i64 u0xe741cca7f5cba874, ; 961: Microsoft.Agents.AI.Abstractions.dll => 182
	i64 u0xe7bea09c4900a191, ; 962: Xamarin.AndroidX.VectorDrawable.dll => 295
	i64 u0xe7e03cc18dcdeb49, ; 963: lib_System.Diagnostics.StackTrace.dll.so => 32
	i64 u0xe7e147ff99a7a380, ; 964: lib_System.Configuration.dll.so => 21
	i64 u0xe8397cf3948e7cb7, ; 965: lib_Microsoft.Extensions.Options.ConfigurationExtensions.dll.so => 203
	i64 u0xe86b0df4ba9e5db8, ; 966: lib_Xamarin.AndroidX.Lifecycle.Runtime.Android.dll.so => 272
	i64 u0xe896622fe0902957, ; 967: System.Reflection.Emit.dll => 95
	i64 u0xe89a2a9ef110899b, ; 968: System.Drawing.dll => 38
	i64 u0xe8c5f8c100b5934b, ; 969: Microsoft.Win32.Registry => 7
	i64 u0xe8ebb412f2f4ac2e, ; 970: Microsoft.Agents.AI.OpenAI => 183
	i64 u0xe957c3976986ab72, ; 971: lib_Xamarin.AndroidX.Window.Extensions.Core.Core.dll.so => 301
	i64 u0xe98163eb702ae5c5, ; 972: Xamarin.AndroidX.Arch.Core.Runtime => 241
	i64 u0xe994f23ba4c143e5, ; 973: Xamarin.KotlinX.Coroutines.Android => 313
	i64 u0xe9b9c8c0458fd92a, ; 974: System.Windows => 157
	i64 u0xe9d166d87a7f2bdb, ; 975: lib_Xamarin.AndroidX.Startup.StartupRuntime.dll.so => 291
	i64 u0xea5a4efc2ad81d1b, ; 976: Xamarin.Google.ErrorProne.Annotations => 305
	i64 u0xeb2313fe9d65b785, ; 977: Xamarin.AndroidX.ConstraintLayout.dll => 248
	i64 u0xec4a53ebd26146c8, ; 978: IdeaBranch.Infrastructure.dll => 354
	i64 u0xed19c616b3fcb7eb, ; 979: Xamarin.AndroidX.VersionedParcelable.dll => 297
	i64 u0xed2e9c1f0f805f0a, ; 980: Microsoft.Agents.AI.dll => 181
	i64 u0xedc4817167106c23, ; 981: System.Net.Sockets.dll => 78
	i64 u0xedc632067fb20ff3, ; 982: System.Memory.dll => 65
	i64 u0xedc8e4ca71a02a8b, ; 983: Xamarin.AndroidX.Navigation.Runtime.dll => 282
	i64 u0xee81f5b3f1c4f83b, ; 984: System.Threading.ThreadPool => 149
	i64 u0xeeb7ebb80150501b, ; 985: lib_Xamarin.AndroidX.Collection.Jvm.dll.so => 245
	i64 u0xeefc635595ef57f0, ; 986: System.Security.Cryptography.Cng => 123
	i64 u0xef03b1b5a04e9709, ; 987: System.Text.Encoding.CodePages.dll => 136
	i64 u0xef602c523fe2e87a, ; 988: lib_Xamarin.Google.Guava.ListenableFuture.dll.so => 307
	i64 u0xef72742e1bcca27a, ; 989: Microsoft.Maui.Essentials.dll => 211
	i64 u0xefd1e0c4e5c9b371, ; 990: System.Resources.ResourceManager.dll => 102
	i64 u0xefe8f8d5ed3c72ea, ; 991: System.Formats.Tar.dll => 41
	i64 u0xefec0b7fdc57ec42, ; 992: Xamarin.AndroidX.Activity => 233
	i64 u0xf00c29406ea45e19, ; 993: es/Microsoft.Maui.Controls.resources.dll => 324
	i64 u0xf09e47b6ae914f6e, ; 994: System.Net.NameResolution => 70
	i64 u0xf0ac2b489fed2e35, ; 995: lib_System.Diagnostics.Debug.dll.so => 28
	i64 u0xf0bb49dadd3a1fe1, ; 996: lib_System.Net.ServicePoint.dll.so => 77
	i64 u0xf0de2537ee19c6ca, ; 997: lib_System.Net.WebHeaderCollection.dll.so => 80
	i64 u0xf1138779fa181c68, ; 998: lib_Xamarin.AndroidX.Lifecycle.Runtime.dll.so => 271
	i64 u0xf11b621fc87b983f, ; 999: Microsoft.Maui.Controls.Xaml.dll => 209
	i64 u0xf161f4f3c3b7e62c, ; 1000: System.Data => 26
	i64 u0xf16eb650d5a464bc, ; 1001: System.ValueTuple => 154
	i64 u0xf1c4b4005493d871, ; 1002: System.Formats.Asn1.dll => 40
	i64 u0xf238bd79489d3a96, ; 1003: lib-nl-Microsoft.Maui.Controls.resources.dll.so => 337
	i64 u0xf2feea356ba760af, ; 1004: Xamarin.AndroidX.Arch.Core.Runtime.dll => 241
	i64 u0xf300e085f8acd238, ; 1005: lib_System.ServiceProcess.dll.so => 135
	i64 u0xf34e52b26e7e059d, ; 1006: System.Runtime.CompilerServices.VisualC.dll => 105
	i64 u0xf356a8f6b6eda61e, ; 1007: lib_Microsoft.Agents.AI.Abstractions.dll.so => 182
	i64 u0xf37221fda4ef8830, ; 1008: lib_Xamarin.Google.Android.Material.dll.so => 302
	i64 u0xf3ad9b8fb3eefd12, ; 1009: lib_System.IO.UnmanagedMemoryStream.dll.so => 59
	i64 u0xf3ddfe05336abf29, ; 1010: System => 167
	i64 u0xf408654b2a135055, ; 1011: System.Reflection.Emit.ILGeneration.dll => 93
	i64 u0xf4103170a1de5bd0, ; 1012: System.Linq.Queryable.dll => 63
	i64 u0xf42d20c23173d77c, ; 1013: lib_System.ServiceModel.Web.dll.so => 134
	i64 u0xf4727d423e5d26f3, ; 1014: SkiaSharp => 217
	i64 u0xf4c1dd70a5496a17, ; 1015: System.IO.Compression => 48
	i64 u0xf4ecf4b9afc64781, ; 1016: System.ServiceProcess.dll => 135
	i64 u0xf4eeeaa566e9b970, ; 1017: lib_Xamarin.AndroidX.CustomView.PoolingContainer.dll.so => 256
	i64 u0xf518f63ead11fcd1, ; 1018: System.Threading.Tasks => 147
	i64 u0xf5fc7602fe27b333, ; 1019: System.Net.WebHeaderCollection => 80
	i64 u0xf6077741019d7428, ; 1020: Xamarin.AndroidX.CoordinatorLayout => 250
	i64 u0xf6742cbf457c450b, ; 1021: Xamarin.AndroidX.Lifecycle.Runtime.Android.dll => 272
	i64 u0xf6f893f692f8cb43, ; 1022: Microsoft.Extensions.Options.ConfigurationExtensions.dll => 203
	i64 u0xf70c0a7bf8ccf5af, ; 1023: System.Web => 156
	i64 u0xf77b20923f07c667, ; 1024: de/Microsoft.Maui.Controls.resources.dll => 322
	i64 u0xf7e2cac4c45067b3, ; 1025: lib_System.Numerics.Vectors.dll.so => 85
	i64 u0xf7e74930e0e3d214, ; 1026: zh-HK/Microsoft.Maui.Controls.resources.dll => 349
	i64 u0xf84773b5c81e3cef, ; 1027: lib-uk-Microsoft.Maui.Controls.resources.dll.so => 347
	i64 u0xf87e0d5910d5cb96, ; 1028: lib_Microsoft.Extensions.Http.Polly.dll.so => 198
	i64 u0xf8aac5ea82de1348, ; 1029: System.Linq.Queryable => 63
	i64 u0xf8b77539b362d3ba, ; 1030: lib_System.Reflection.Primitives.dll.so => 98
	i64 u0xf8e045dc345b2ea3, ; 1031: lib_Xamarin.AndroidX.RecyclerView.dll.so => 285
	i64 u0xf915dc29808193a1, ; 1032: System.Web.HttpUtility.dll => 155
	i64 u0xf96c777a2a0686f4, ; 1033: hi/Microsoft.Maui.Controls.resources.dll => 328
	i64 u0xf9be54c8bcf8ff3b, ; 1034: System.Security.AccessControl.dll => 120
	i64 u0xf9eec5bb3a6aedc6, ; 1035: Microsoft.Extensions.Options => 202
	i64 u0xfa0e82300e67f913, ; 1036: lib_System.AppContext.dll.so => 8
	i64 u0xfa2fdb27e8a2c8e8, ; 1037: System.ComponentModel.EventBasedAsync => 17
	i64 u0xfa3f278f288b0e84, ; 1038: lib_System.Net.Security.dll.so => 76
	i64 u0xfa5af62c7d2e663d, ; 1039: Microsoft.Extensions.AI => 186
	i64 u0xfa5ed7226d978949, ; 1040: lib-ar-Microsoft.Maui.Controls.resources.dll.so => 318
	i64 u0xfa645d91e9fc4cba, ; 1041: System.Threading.Thread => 148
	i64 u0xfa99d44ebf9bea5b, ; 1042: SkiaSharp.Views.Maui.Core => 220
	i64 u0xfad4d2c770e827f9, ; 1043: lib_System.IO.IsolatedStorage.dll.so => 54
	i64 u0xfb022853d73b7fa5, ; 1044: lib_SQLitePCLRaw.batteries_v2.dll.so => 222
	i64 u0xfb06dd2338e6f7c4, ; 1045: System.Net.Ping.dll => 72
	i64 u0xfb087abe5365e3b7, ; 1046: lib_System.Data.DataSetExtensions.dll.so => 25
	i64 u0xfb846e949baff5ea, ; 1047: System.Xml.Serialization.dll => 160
	i64 u0xfbad3e4ce4b98145, ; 1048: System.Security.Cryptography.X509Certificates => 128
	i64 u0xfbf0a31c9fc34bc4, ; 1049: lib_System.Net.Http.dll.so => 67
	i64 u0xfc6b7527cc280b3f, ; 1050: lib_System.Runtime.Serialization.Formatters.dll.so => 114
	i64 u0xfc719aec26adf9d9, ; 1051: Xamarin.AndroidX.Navigation.Fragment.dll => 281
	i64 u0xfc82690c2fe2735c, ; 1052: Xamarin.AndroidX.Lifecycle.Process.dll => 270
	i64 u0xfc93fc307d279893, ; 1053: System.IO.Pipes.AccessControl.dll => 57
	i64 u0xfcd302092ada6328, ; 1054: System.IO.MemoryMappedFiles.dll => 55
	i64 u0xfd22f00870e40ae0, ; 1055: lib_Xamarin.AndroidX.DrawerLayout.dll.so => 257
	i64 u0xfd49b3c1a76e2748, ; 1056: System.Runtime.InteropServices.RuntimeInformation => 109
	i64 u0xfd4f1cb508086944, ; 1057: Microsoft.Extensions.Http.Polly => 198
	i64 u0xfd536c702f64dc47, ; 1058: System.Text.Encoding.Extensions => 137
	i64 u0xfd583f7657b6a1cb, ; 1059: Xamarin.AndroidX.Fragment => 262
	i64 u0xfd8dd91a2c26bd5d, ; 1060: Xamarin.AndroidX.Lifecycle.Runtime => 271
	i64 u0xfda36abccf05cf5c, ; 1061: System.Net.WebSockets.Client => 82
	i64 u0xfdbe4710aa9beeff, ; 1062: CommunityToolkit.Maui => 178
	i64 u0xfddbe9695626a7f5, ; 1063: Xamarin.AndroidX.Lifecycle.Common => 265
	i64 u0xfdf41d091d68de19, ; 1064: Microsoft.Agents.AI => 181
	i64 u0xfeae9952cf03b8cb, ; 1065: tr/Microsoft.Maui.Controls.resources => 346
	i64 u0xfebe1950717515f9, ; 1066: Xamarin.AndroidX.Lifecycle.LiveData.Core.Ktx.dll => 269
	i64 u0xff270a55858bac8d, ; 1067: System.Security.Principal => 131
	i64 u0xff9b54613e0d2cc8, ; 1068: System.Net.Http.Json => 66
	i64 u0xffdb7a971be4ec73, ; 1069: System.ValueTuple.dll => 154
	i64 u0xfff40914e0b38d3d ; 1070: Azure.Identity.dll => 177
], align 16

@assembly_image_cache_indices = dso_local local_unnamed_addr constant [1071 x i32] [
	i32 44, i32 314, i32 292, i32 213, i32 15, i32 282, i32 179, i32 203,
	i32 107, i32 173, i32 50, i32 238, i32 9, i32 225, i32 88, i32 342,
	i32 320, i32 348, i32 207, i32 258, i32 73, i32 285, i32 185, i32 14,
	i32 210, i32 104, i32 185, i32 349, i32 158, i32 21, i32 263, i32 245,
	i32 163, i32 260, i32 295, i32 169, i32 342, i32 12, i32 201, i32 296,
	i32 176, i32 98, i32 256, i32 257, i32 15, i32 202, i32 12, i32 227,
	i32 129, i32 97, i32 189, i32 142, i32 41, i32 343, i32 317, i32 298,
	i32 339, i32 174, i32 232, i32 7, i32 211, i32 69, i32 289, i32 132,
	i32 184, i32 288, i32 259, i32 70, i32 218, i32 246, i32 68, i32 59,
	i32 2, i32 184, i32 255, i32 54, i32 45, i32 127, i32 69, i32 83,
	i32 273, i32 160, i32 94, i32 101, i32 285, i32 143, i32 153, i32 242,
	i32 326, i32 164, i32 171, i32 327, i32 194, i32 83, i32 309, i32 246,
	i32 6, i32 7, i32 53, i32 103, i32 58, i32 122, i32 100, i32 170,
	i32 120, i32 314, i32 23, i32 330, i32 139, i32 215, i32 99, i32 317,
	i32 79, i32 336, i32 291, i32 121, i32 177, i32 10, i32 167, i32 345,
	i32 72, i32 231, i32 274, i32 286, i32 195, i32 173, i32 147, i32 42,
	i32 289, i32 49, i32 32, i32 283, i32 334, i32 146, i32 202, i32 165,
	i32 30, i32 86, i32 293, i32 79, i32 45, i32 31, i32 44, i32 105,
	i32 119, i32 236, i32 226, i32 47, i32 93, i32 345, i32 58, i32 150,
	i32 148, i32 102, i32 51, i32 22, i32 251, i32 188, i32 116, i32 229,
	i32 326, i32 304, i32 222, i32 310, i32 204, i32 96, i32 60, i32 331,
	i32 329, i32 83, i32 304, i32 171, i32 28, i32 73, i32 284, i32 196,
	i32 217, i32 261, i32 347, i32 71, i32 35, i32 325, i32 16, i32 141,
	i32 40, i32 188, i32 351, i32 247, i32 338, i32 136, i32 94, i32 90,
	i32 151, i32 344, i32 26, i32 140, i32 59, i32 53, i32 323, i32 31,
	i32 159, i32 36, i32 166, i32 189, i32 197, i32 262, i32 207, i32 54,
	i32 356, i32 300, i32 92, i32 306, i32 215, i32 243, i32 37, i32 219,
	i32 326, i32 159, i32 11, i32 324, i32 78, i32 57, i32 210, i32 320,
	i32 208, i32 15, i32 299, i32 190, i32 240, i32 111, i32 277, i32 34,
	i32 106, i32 86, i32 94, i32 55, i32 98, i32 308, i32 60, i32 11,
	i32 104, i32 255, i32 70, i32 298, i32 319, i32 214, i32 127, i32 286,
	i32 118, i32 137, i32 128, i32 108, i32 310, i32 133, i32 242, i32 307,
	i32 149, i32 158, i32 263, i32 251, i32 222, i32 258, i32 286, i32 99,
	i32 26, i32 216, i32 290, i32 206, i32 145, i32 280, i32 177, i32 220,
	i32 5, i32 169, i32 352, i32 239, i32 102, i32 163, i32 101, i32 253,
	i32 27, i32 95, i32 170, i32 174, i32 234, i32 5, i32 338, i32 260,
	i32 3, i32 116, i32 310, i32 263, i32 270, i32 35, i32 8, i32 342,
	i32 158, i32 227, i32 340, i32 55, i32 87, i32 297, i32 283, i32 46,
	i32 269, i32 228, i32 106, i32 0, i32 49, i32 140, i32 226, i32 218,
	i32 66, i32 279, i32 71, i32 82, i32 61, i32 91, i32 156, i32 226,
	i32 240, i32 135, i32 112, i32 332, i32 279, i32 284, i32 173, i32 136,
	i32 142, i32 42, i32 319, i32 224, i32 192, i32 208, i32 182, i32 62,
	i32 192, i32 276, i32 81, i32 27, i32 38, i32 101, i32 273, i32 73,
	i32 24, i32 251, i32 212, i32 343, i32 123, i32 71, i32 109, i32 349,
	i32 121, i32 183, i32 119, i32 265, i32 266, i32 13, i32 4, i32 126,
	i32 220, i32 117, i32 187, i32 144, i32 214, i32 43, i32 89, i32 235,
	i32 223, i32 175, i32 29, i32 150, i32 192, i32 333, i32 193, i32 305,
	i32 234, i32 355, i32 3, i32 236, i32 227, i32 46, i32 250, i32 151,
	i32 20, i32 88, i32 219, i32 321, i32 43, i32 269, i32 244, i32 274,
	i32 96, i32 199, i32 30, i32 43, i32 80, i32 259, i32 247, i32 146,
	i32 110, i32 245, i32 13, i32 107, i32 139, i32 18, i32 124, i32 68,
	i32 213, i32 159, i32 24, i32 224, i32 323, i32 316, i32 104, i32 193,
	i32 315, i32 65, i32 60, i32 209, i32 322, i32 112, i32 175, i32 352,
	i32 313, i32 11, i32 302, i32 122, i32 100, i32 107, i32 277, i32 178,
	i32 208, i32 113, i32 237, i32 51, i32 22, i32 276, i32 254, i32 74,
	i32 249, i32 157, i32 41, i32 321, i32 37, i32 311, i32 40, i32 327,
	i32 224, i32 301, i32 110, i32 354, i32 336, i32 1, i32 23, i32 308,
	i32 2, i32 275, i32 212, i32 17, i32 204, i32 81, i32 81, i32 254,
	i32 204, i32 281, i32 288, i32 154, i32 23, i32 210, i32 320, i32 187,
	i32 52, i32 53, i32 346, i32 336, i32 96, i32 230, i32 196, i32 332,
	i32 18, i32 253, i32 0, i32 125, i32 329, i32 162, i32 186, i32 47,
	i32 305, i32 180, i32 118, i32 65, i32 168, i32 195, i32 198, i32 188,
	i32 190, i32 16, i32 287, i32 113, i32 237, i32 62, i32 312, i32 123,
	i32 335, i32 4, i32 345, i32 262, i32 275, i32 197, i32 311, i32 309,
	i32 275, i32 8, i32 244, i32 325, i32 258, i32 185, i32 19, i32 343,
	i32 322, i32 79, i32 248, i32 179, i32 133, i32 308, i32 335, i32 85,
	i32 353, i32 201, i32 14, i32 36, i32 121, i32 316, i32 270, i32 260,
	i32 87, i32 229, i32 20, i32 298, i32 191, i32 205, i32 268, i32 74,
	i32 97, i32 225, i32 167, i32 264, i32 84, i32 351, i32 238, i32 243,
	i32 312, i32 156, i32 38, i32 153, i32 347, i32 221, i32 207, i32 350,
	i32 146, i32 58, i32 115, i32 217, i32 244, i32 295, i32 294, i32 39,
	i32 351, i32 190, i32 117, i32 236, i32 16, i32 230, i32 148, i32 45,
	i32 211, i32 234, i32 100, i32 315, i32 170, i32 18, i32 50, i32 109,
	i32 99, i32 189, i32 279, i32 29, i32 130, i32 31, i32 327, i32 355,
	i32 176, i32 288, i32 130, i32 46, i32 254, i32 259, i32 151, i32 10,
	i32 205, i32 280, i32 328, i32 341, i32 223, i32 340, i32 134, i32 339,
	i32 44, i32 316, i32 223, i32 35, i32 356, i32 48, i32 145, i32 276,
	i32 209, i32 267, i32 255, i32 0, i32 140, i32 187, i32 1, i32 64,
	i32 134, i32 319, i32 50, i32 162, i32 241, i32 267, i32 355, i32 230,
	i32 265, i32 335, i32 294, i32 48, i32 166, i32 264, i32 324, i32 261,
	i32 206, i32 331, i32 212, i32 20, i32 10, i32 180, i32 252, i32 126,
	i32 61, i32 143, i32 282, i32 334, i32 271, i32 303, i32 300, i32 152,
	i32 144, i32 314, i32 311, i32 128, i32 313, i32 162, i32 164, i32 214,
	i32 256, i32 233, i32 191, i32 337, i32 28, i32 353, i32 280, i32 216,
	i32 268, i32 205, i32 84, i32 300, i32 129, i32 304, i32 103, i32 150,
	i32 302, i32 283, i32 56, i32 164, i32 218, i32 169, i32 133, i32 39,
	i32 296, i32 334, i32 195, i32 24, i32 114, i32 92, i32 52, i32 62,
	i32 124, i32 85, i32 129, i32 165, i32 303, i32 168, i32 287, i32 289,
	i32 257, i32 229, i32 272, i32 6, i32 266, i32 330, i32 172, i32 4,
	i32 277, i32 118, i32 235, i32 21, i32 179, i32 200, i32 91, i32 67,
	i32 32, i32 194, i32 323, i32 249, i32 61, i32 113, i32 268, i32 34,
	i32 130, i32 161, i32 341, i32 247, i32 142, i32 337, i32 155, i32 19,
	i32 246, i32 232, i32 77, i32 76, i32 17, i32 354, i32 171, i32 87,
	i32 221, i32 312, i32 126, i32 267, i32 278, i32 248, i32 344, i32 228,
	i32 274, i32 36, i32 120, i32 141, i32 124, i32 108, i32 321, i32 296,
	i32 243, i32 328, i32 318, i32 56, i32 49, i32 30, i32 147, i32 200,
	i32 149, i32 37, i32 344, i32 175, i32 197, i32 301, i32 77, i32 353,
	i32 163, i32 3, i32 290, i32 340, i32 333, i32 161, i32 14, i32 157,
	i32 153, i32 78, i32 206, i32 105, i32 114, i32 225, i32 240, i32 67,
	i32 68, i32 299, i32 47, i32 242, i32 111, i32 9, i32 239, i32 57,
	i32 235, i32 66, i32 318, i32 252, i32 22, i32 111, i32 103, i32 64,
	i32 144, i32 183, i32 233, i32 9, i32 333, i32 172, i32 52, i32 299,
	i32 117, i32 143, i32 180, i32 168, i32 82, i32 115, i32 278, i32 19,
	i32 75, i32 281, i32 91, i32 231, i32 89, i32 122, i32 293, i32 237,
	i32 137, i32 155, i32 108, i32 13, i32 92, i32 186, i32 33, i32 178,
	i32 346, i32 138, i32 338, i32 306, i32 341, i32 291, i32 232, i32 42,
	i32 356, i32 215, i32 290, i32 184, i32 141, i32 315, i32 317, i32 27,
	i32 350, i32 75, i32 264, i32 292, i32 176, i32 29, i32 69, i32 90,
	i32 97, i32 115, i32 33, i32 106, i32 266, i32 39, i32 196, i32 74,
	i32 307, i32 228, i32 110, i32 125, i32 239, i32 89, i32 199, i32 88,
	i32 332, i32 95, i32 194, i32 131, i32 278, i32 2, i32 293, i32 1,
	i32 201, i32 287, i32 252, i32 292, i32 249, i32 306, i32 303, i32 191,
	i32 165, i32 132, i32 200, i32 297, i32 284, i32 193, i32 12, i32 51,
	i32 348, i32 93, i32 219, i32 348, i32 152, i32 64, i32 138, i32 152,
	i32 63, i32 199, i32 119, i32 139, i32 309, i32 86, i32 350, i32 213,
	i32 161, i32 294, i32 145, i32 329, i32 261, i32 84, i32 72, i32 238,
	i32 138, i32 250, i32 231, i32 127, i32 181, i32 352, i32 56, i32 112,
	i32 132, i32 90, i32 25, i32 76, i32 131, i32 33, i32 75, i32 273,
	i32 331, i32 160, i32 25, i32 6, i32 172, i32 339, i32 221, i32 125,
	i32 253, i32 330, i32 325, i32 116, i32 174, i32 34, i32 5, i32 216,
	i32 166, i32 182, i32 295, i32 32, i32 21, i32 203, i32 272, i32 95,
	i32 38, i32 7, i32 183, i32 301, i32 241, i32 313, i32 157, i32 291,
	i32 305, i32 248, i32 354, i32 297, i32 181, i32 78, i32 65, i32 282,
	i32 149, i32 245, i32 123, i32 136, i32 307, i32 211, i32 102, i32 41,
	i32 233, i32 324, i32 70, i32 28, i32 77, i32 80, i32 271, i32 209,
	i32 26, i32 154, i32 40, i32 337, i32 241, i32 135, i32 105, i32 182,
	i32 302, i32 59, i32 167, i32 93, i32 63, i32 134, i32 217, i32 48,
	i32 135, i32 256, i32 147, i32 80, i32 250, i32 272, i32 203, i32 156,
	i32 322, i32 85, i32 349, i32 347, i32 198, i32 63, i32 98, i32 285,
	i32 155, i32 328, i32 120, i32 202, i32 8, i32 17, i32 76, i32 186,
	i32 318, i32 148, i32 220, i32 54, i32 222, i32 72, i32 25, i32 160,
	i32 128, i32 67, i32 114, i32 281, i32 270, i32 57, i32 55, i32 257,
	i32 109, i32 198, i32 137, i32 262, i32 271, i32 82, i32 178, i32 265,
	i32 181, i32 346, i32 269, i32 131, i32 66, i32 154, i32 177
], align 16

@marshal_methods_number_of_classes = dso_local local_unnamed_addr constant i32 0, align 4

@marshal_methods_class_cache = dso_local local_unnamed_addr global [0 x %struct.MarshalMethodsManagedClass] zeroinitializer, align 8

; Names of classes in which marshal methods reside
@mm_class_names = dso_local local_unnamed_addr constant [0 x ptr] zeroinitializer, align 8

@mm_method_names = dso_local local_unnamed_addr constant [1 x %struct.MarshalMethodName] [
	%struct.MarshalMethodName {
		i64 u0x0000000000000000, ; name: 
		ptr @.MarshalMethodName.0_name; char* name
	} ; 0
], align 8

; get_function_pointer (uint32_t mono_image_index, uint32_t class_index, uint32_t method_token, void*& target_ptr)
@get_function_pointer = internal dso_local unnamed_addr global ptr null, align 8

; Functions

; Function attributes: memory(write, argmem: none, inaccessiblemem: none) "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" nofree norecurse nosync nounwind "stack-protector-buffer-size"="8" uwtable willreturn
define void @xamarin_app_init(ptr nocapture noundef readnone %env, ptr noundef %fn) local_unnamed_addr #0
{
	%fnIsNull = icmp eq ptr %fn, null
	br i1 %fnIsNull, label %1, label %2

1: ; preds = %0
	%putsResult = call noundef i32 @puts(ptr @.str.0)
	call void @abort()
	unreachable 

2: ; preds = %1, %0
	store ptr %fn, ptr @get_function_pointer, align 8, !tbaa !3
	ret void
}

; Strings
@.str.0 = private unnamed_addr constant [40 x i8] c"get_function_pointer MUST be specified\0A\00", align 16

;MarshalMethodName
@.MarshalMethodName.0_name = private unnamed_addr constant [1 x i8] c"\00", align 1

; External functions

; Function attributes: "no-trapping-math"="true" noreturn nounwind "stack-protector-buffer-size"="8"
declare void @abort() local_unnamed_addr #2

; Function attributes: nofree nounwind
declare noundef i32 @puts(ptr noundef) local_unnamed_addr #1
attributes #0 = { memory(write, argmem: none, inaccessiblemem: none) "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" nofree norecurse nosync nounwind "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+crc32,+cx16,+cx8,+fxsr,+mmx,+popcnt,+sse,+sse2,+sse3,+sse4.1,+sse4.2,+ssse3,+x87" "tune-cpu"="generic" uwtable willreturn }
attributes #1 = { nofree nounwind }
attributes #2 = { "no-trapping-math"="true" noreturn nounwind "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+crc32,+cx16,+cx8,+fxsr,+mmx,+popcnt,+sse,+sse2,+sse3,+sse4.1,+sse4.2,+ssse3,+x87" "tune-cpu"="generic" }

; Metadata
!llvm.module.flags = !{!0, !1}
!0 = !{i32 1, !"wchar_size", i32 4}
!1 = !{i32 7, !"PIC Level", i32 2}
!llvm.ident = !{!2}
!2 = !{!".NET for Android remotes/origin/release/9.0.1xx @ 1dcfb6f8779c33b6f768c996495cb90ecd729329"}
!3 = !{!4, !4, i64 0}
!4 = !{!"any pointer", !5, i64 0}
!5 = !{!"omnipotent char", !6, i64 0}
!6 = !{!"Simple C++ TBAA"}
