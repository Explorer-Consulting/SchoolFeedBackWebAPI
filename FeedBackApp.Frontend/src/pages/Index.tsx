import { GoogleLogin, CredentialResponse } from '@react-oauth/google'
import { useNavigate } from 'react-router-dom'
import { useReviews } from '@/hooks/useReviews'
import { useAuthStore } from '@/hooks/useAuth'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { Loader2, Mail } from 'lucide-react'
import { User } from '@/models/User'
import { FaFacebookF, FaMicrosoft, FaLinkedinIn } from "react-icons/fa";
//import { useFacebook } from "@/hooks/useFacebook";
import { useState } from 'react'
import { useToast } from '@/hooks/useToast'
import { PublicClientApplication } from "@azure/msal-browser";

export default function SocialAuthApp() {
  const navigate = useNavigate()
  const setUser = useAuthStore((state) => state.setUser)
  const [email, setEmail] = useState('')
  const { toast } = useToast()

  const {
    sendOTP,
    isSendingOTP,
    loginWithGoogle,
    //loginWithFacebook,
    loginWithMicrosoft,
    loginWithLinkedIn,
    isLoggingIn,
    //isLoggingInFacebook,
    isLoggingInMicrosoft,
    isLoggingInLinkedIn
  } = useReviews()

  const handleSuccess = (user: any) => {
    setUser(user)

    const params = new URLSearchParams(window.location.search)
    const returnTo = params.get('returnTo')
    if (returnTo) {
      navigate(returnTo)
      return
    }

    if (user.role === 'Admin') {
      navigate("/dashboard/admin")
    } else if (user.role === 'Student') {
      navigate("/dashboard/student/")
    } else {
      navigate("/no-access")
    }
  }

  const handleError = (e: any) => {
    if (e.response?.status === 403) {
      navigate("/no-access")
    } else {
      navigate("/no-access")
      console.error(e)
    }
  }

  const enabledProviders = (import.meta.env.VITE_ENABLED_LOGIN_PROVIDERS || "")
                            .split(",")
                            .map((p) => p.trim());

  const onGoogleSuccess = (resp: CredentialResponse) => {
    const idToken = resp?.credential;
    if (!idToken) return console.error("No ID token from Google");

    loginWithGoogle(idToken, {
      onSuccess: handleSuccess,
      onError: handleError,
    });
  };

  /*const FACEBOOK_APP_ID = import.meta.env.VITE_FACEBOOK_APP_ID;
  const fbLoaded = useFacebook(FACEBOOK_APP_ID);

   const onFacebookLogin = () => {
    if (!fbLoaded || !window.FB) {
      alert("Facebook SDK nem töltődött be, ellenőrizd a böngésződ!");
      return;
    }

    window.FB.login(
      (response: any) => {
        if (response.authResponse) {
          loginWithFacebook(response.authResponse.accessToken, {
            onSuccess: handleSuccess,
            onError: handleError,
          });
        } else {
          console.error("Facebook login cancelled");
        }
      },
      { scope: "email,public_profile" }
    );
  };
  */



const onMicrosoftLogin = async () => {
  try {
    // 1. Létrehozzuk a példányt
    const tenantId = import.meta.env.VITE_MICROSOFT_TENANT_ID;
    const msalInstance = new PublicClientApplication({
      auth: {
        clientId: import.meta.env.VITE_MICROSOFT_CLIENT_ID,
        authority: `https://login.microsoftonline.com/${tenantId}`,
        redirectUri: window.location.origin,
      },
    });

    // 2. Inicializáljuk az MSAL-t
    await msalInstance.initialize();

    // 3. Popup login
    const response = await msalInstance.loginPopup({
      scopes: ["openid", "profile", "email", "User.Read"],
    });

    const idToken = response.idToken;
    if (!idToken) throw new Error("No idToken received");

    loginWithMicrosoft(idToken, {
      onSuccess: handleSuccess,
      onError: handleError,
    });

  } catch (err) {
    console.error("Microsoft login cancelled or failed", err);
  }
};


  const onLinkedInLogin = () => {
    const accessToken = "linkedin_access_token" // Replace with real token from LinkedIn OAuth
    loginWithLinkedIn(accessToken, { onSuccess: handleSuccess, onError: handleError })
  }

  const handleEmailLogin = async () => {
    if (!email || !email.includes('@')) {
      toast({
        title: "Hiba",
        description: "Kérjük, adjon meg egy érvényes email címet",
        variant: "destructive"
      })
      return
    }

    sendOTP(email, {
      onSuccess: () => {
        toast({
          title: "Email elküldve",
          description: `Az OTP kódot elküldtük a következő címre: ${email}`,
        })
        
        // Wait 3 seconds then redirect
        setTimeout(() => {
          navigate('/passwordless-otp-login', { state: { email } })
        }, 3000)
      },
      onError: (e: any) => {
        toast({
          title: "Hiba",
          description: e.response?.data?.message || "Nem sikerült elküldeni az emailt. Kérjük, próbálja újra.",
          variant: "destructive"
        })
      }
    })
  }

  return (
    <main className="min-h-screen grid place-items-center px-4">
      <Card className="w-full max-w-sm">
        <CardHeader className="text-center">
          <CardTitle className="text-2xl">Bejelentkezés</CardTitle>
          <p className="text-sm text-muted-foreground">
            Jelentkezz be egy közösségi fiókkal
          </p>
        </CardHeader>
        <CardContent className="flex flex-col items-center gap-4">
          {enabledProviders.includes("Google") && (
          <GoogleLogin
            onSuccess={onGoogleSuccess}
            onError={() => console.error("Google login failed")}
            useOneTap
            auto_select
            theme="outline"
            size="large"
            shape="pill"
            text="continue_with"
            logo_alignment="center"
            width="280"
          />
          )}
          {/*
          <button
            onClick={onFacebookLogin}
            className="flex items-center justify-center gap-3 bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 px-4 rounded-full shadow-md transition-all duration-300 w-full"
          >
            <FaFacebookF className="w-5 h-5" />
            Facebook
          </button>
          */}
          {enabledProviders.includes("Microsoft") && (
          <button
            onClick={onMicrosoftLogin}
            className="flex items-center justify-center gap-3 bg-gray-800 hover:bg-gray-900 text-white font-medium py-2 px-4 rounded-full shadow-md transition-all duration-300 w-full"
          >
            <FaMicrosoft className="w-5 h-5" />
            Microsoft
          </button>
          )}
          {/*
          <button
            onClick={onLinkedInLogin}
            className="flex items-center justify-center gap-3 bg-blue-500 hover:bg-blue-600 text-white font-medium py-2 px-4 rounded-full shadow-md transition-all duration-300 w-full"
          >
            <FaLinkedinIn className="w-5 h-5" />
            LinkedIn
          </button>
          */}

          {isLoggingIn && <Status text="Google bejelentkezés folyamatban…" />}
          {/*{isLoggingInFacebook && <Status text="Facebook bejelentkezés folyamatban…" />}*/}
          {isLoggingInMicrosoft && <Status text="Microsoft bejelentkezés folyamatban…" />}
          {isLoggingInLinkedIn && <Status text="LinkedIn bejelentkezés folyamatban…" />}
        </CardContent>

        <div className="w-full border-t pt-6">
          <CardHeader className="text-center pb-4">
            <p className="text-sm text-muted-foreground">
              Jelentkezz be Email címmel jelszó nélkül
            </p>
          </CardHeader>
          <CardContent className="flex flex-col gap-4">
            <div className="relative">
              <Mail className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input
                type="email"
                placeholder="example@gmail.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                onKeyDown={(e) => {
                  if (e.key === 'Enter' && !isSendingOTP) {
                    handleEmailLogin()
                  }
                }}
                className="pl-10"
              />
            </div>
            <Button
              onClick={handleEmailLogin}
              disabled={!email || !email.includes('@') || isSendingOTP}
              className="w-full"
            >
              {isSendingOTP ? (
                <>
                  <Loader2 className="h-4 w-4 animate-spin" />
                  Küldés...
                </>
              ) : (
                'Bejelentkezés'
              )}
            </Button>
          </CardContent>
        </div>
      </Card>
    </main>
  )
}

function Status({ text }: { text: string }) {
  return (
    <div className="inline-flex items-center gap-2 text-sm text-muted-foreground">
      <Loader2 className="h-4 w-4 animate-spin" />
      {text}
    </div>
  )
}