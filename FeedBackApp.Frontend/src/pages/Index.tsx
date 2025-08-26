import { GoogleLogin, CredentialResponse } from '@react-oauth/google'
import { useNavigate } from 'react-router-dom'
import { useReviews } from '@/hooks/useReviews'
import { useAuthStore } from '@/stores/useAuthStore'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Loader2 } from 'lucide-react'
import {User} from '@/models/User'

export default function GoogleAuthApp() {
  const navigate = useNavigate()
  const setUser = useAuthStore((state) => state.setUser)

  const { loginWithGoogle, isLoggingIn } = useReviews()

  const onIdTokenSuccess = (resp: CredentialResponse) => {
    const idToken = resp?.credential
    if (!idToken) {
      console.error("No ID token from Google")
      return
    }

    loginWithGoogle(idToken, {
      onSuccess: (user:User) => {
        setUser(user)
        
        if (user.role === 'Admin') {
          navigate("/dashboard/admin")
        } else if (user.role === 'Student') {
          navigate("/dashboard/student/")
        } else {
          navigate("/no-access")
        }
      },
      onError: (e:any) => {
        if (e.response?.status === 403) {
          navigate("/no-access")
        } else {
          console.error(e)
        }
      }
    })
  }

  return (
    <main className="min-h-screen grid place-items-center px-4">
      <Card className="w-full max-w-sm">
        <CardHeader className="text-center">
          <CardTitle className="text-2xl">Bejelentkezés</CardTitle>
          <p className="text-sm text-muted-foreground">
            Jelentkezz be Google-fiókkal
          </p>
        </CardHeader>
        <CardContent className="flex flex-col items-center gap-4">
          <GoogleLogin
            onSuccess={onIdTokenSuccess}
            onError={() => console.error("Login failed")}
            useOneTap
            auto_select
            theme="outline"
            size="large"
            shape="pill"
            text="continue_with"
            logo_alignment="center"
            width="280"
          />
          {isLoggingIn && (
            <div className="inline-flex items-center gap-2 text-sm text-muted-foreground">
              <Loader2 className="h-4 w-4 animate-spin" />
              Bejelentkezés folyamatban…
            </div>
          )}
        </CardContent>
      </Card>
    </main>
  )
}
