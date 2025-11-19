import { GoogleLogin, CredentialResponse } from '@react-oauth/google'
import { useNavigate } from 'react-router-dom'
import { useReviews } from '@/hooks/useReviews'
import { useAuthStore } from '@/hooks/useAuth'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Loader2 } from 'lucide-react'
import { User } from '@/models/User'

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

  const handleSuccess = (user: User) => {
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
    const idToken = resp?.credential
    if (!idToken) return console.error("No ID token from Google")
    loginWithGoogle(idToken, { onSuccess: handleSuccess, onError: handleError })
  }

  const onFacebookLogin = () => {
    const accessToken = "facebook_access_token" // Replace with real token from Facebook SDK
    loginWithFacebook(accessToken, { onSuccess: handleSuccess, onError: handleError })
  }

  const onMicrosoftLogin = () => {
    const idToken = "microsoft_id_token" // Replace with real token from Microsoft auth
    loginWithMicrosoft(idToken, { onSuccess: handleSuccess, onError: handleError })
  }

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
          <button onClick={onFacebookLogin} className="btn-social facebook">Facebook</button>
          <button onClick={onMicrosoftLogin} className="btn-social microsoft">Microsoft</button>
          <button onClick={onLinkedInLogin} className="btn-social linkedin">LinkedIn</button>

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