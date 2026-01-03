import { GoogleLogin, CredentialResponse } from '@react-oauth/google'
import { useNavigate } from 'react-router-dom'
import { useReviews } from '@/hooks/useReviews'
import { useAuthStore } from '@/hooks/useAuth'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { Loader2, Mail } from 'lucide-react'
import { User } from '@/models/User'
import { useState } from 'react'
import { useToast } from '@/hooks/useToast'

export default function GoogleAuthApp() {
  const navigate = useNavigate()
  const setUser = useAuthStore((state) => state.setUser)
  const [email, setEmail] = useState('')
  const { toast } = useToast()

  const { loginWithGoogle, isLoggingIn, sendOTP, isSendingOTP } = useReviews()

  const onIdTokenSuccess = (resp: CredentialResponse) => {
    const idToken = resp?.credential

    if (!idToken) {
      console.error("No ID token from Google")
      return
    }

    loginWithGoogle(idToken, {
      onSuccess: (user: User) => {
        setUser(user)

        if (user.role === 'Admin') {
          navigate("/dashboard/admin")
        } else if (user.role === 'Student') {
          navigate("/dashboard/student/")
        } else {
          navigate("/no-access")
        }
      },
      onError: (e: any) => {
        if (e.response?.status === 403) {
          navigate("/no-access")
        } else {
          navigate("/no-access")
          console.error(e);
        }
      }
    })
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
