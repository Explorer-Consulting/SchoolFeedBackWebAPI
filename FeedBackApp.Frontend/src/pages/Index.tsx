import { GoogleLogin, CredentialResponse } from '@react-oauth/google'
import { useNavigate } from 'react-router-dom'
import { useReviews } from '@/hooks/useReviews'
import { useAuthStore } from '@/hooks/useAuth'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Loader2 } from 'lucide-react'
import { User } from '@/models/User'
import { FaFacebookF, FaMicrosoft, FaLinkedinIn } from "react-icons/fa";
import { useFacebook } from "@/hooks/useFacebook";
import { PublicClientApplication } from "@azure/msal-browser";

export default function SocialAuthApp() {
  const navigate = useNavigate()
  const setUser = useAuthStore((state) => state.setUser)

  const {
    loginWithGoogle,
    loginWithFacebook,
    loginWithMicrosoft,
    loginWithLinkedIn,
    isLoggingIn,
    isLoggingInFacebook,
    isLoggingInMicrosoft,
    isLoggingInLinkedIn
  } = useReviews()

  const handleSuccess = (user: any) => {
    setUser(user)
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

  const onGoogleSuccess = (resp: CredentialResponse) => {
    const idToken = resp?.credential;
    if (!idToken) return console.error("No ID token from Google");

    loginWithGoogle(idToken, {
      onSuccess: handleSuccess,
      onError: handleError,
    });
  };

  const FACEBOOK_APP_ID = import.meta.env.VITE_FACEBOOK_APP_ID;
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



const onMicrosoftLogin = async () => {
  try {
    // 1. Létrehozzuk a példányt
    const msalInstance = new PublicClientApplication({
      auth: {
        clientId: import.meta.env.VITE_MICROSOFT_CLIENT_ID,
        authority: "https://login.microsoftonline.com/common",
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
          <button
            onClick={onFacebookLogin}
            className="flex items-center justify-center gap-3 bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 px-4 rounded-full shadow-md transition-all duration-300 w-full"
          >
            <FaFacebookF className="w-5 h-5" />
            Facebook
          </button>

          <button
            onClick={onMicrosoftLogin}
            className="flex items-center justify-center gap-3 bg-gray-800 hover:bg-gray-900 text-white font-medium py-2 px-4 rounded-full shadow-md transition-all duration-300 w-full"
          >
            <FaMicrosoft className="w-5 h-5" />
            Microsoft
          </button>
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
          {isLoggingInFacebook && <Status text="Facebook bejelentkezés folyamatban…" />}
          {isLoggingInMicrosoft && <Status text="Microsoft bejelentkezés folyamatban…" />}
          {isLoggingInLinkedIn && <Status text="LinkedIn bejelentkezés folyamatban…" />}
        </CardContent>
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