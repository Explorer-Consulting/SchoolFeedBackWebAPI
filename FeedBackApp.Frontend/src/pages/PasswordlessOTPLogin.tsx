import { useNavigate, useLocation } from 'react-router-dom'
import { useReviews } from '@/hooks/useReviews'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { Loader2, Mail, ArrowLeft } from 'lucide-react'
import { useState, useEffect } from 'react'
import { useToast } from '@/hooks/useToast'

export default function PasswordlessOTPLogin() {
  const navigate = useNavigate()
  const location = useLocation()
  const [otp, setOtp] = useState('')
  const [email, setEmail] = useState<string>('')
  const { toast } = useToast()

  const { sendOTP, isSendingOTP } = useReviews()

  useEffect(() => {
    // Get email from navigation state
    const stateEmail = location.state?.email
    if (stateEmail) {
      setEmail(stateEmail)
    } else {
      // If no email in state, redirect back to home
      navigate('/')
    }
  }, [location.state, navigate])

  const handleResendOTP = () => {
    if (!email) return

    sendOTP(email, {
      onSuccess: () => {
        toast({
          title: "Új kód elküldve",
          description: `Az új OTP kódot elküldtük a következő címre: ${email}`,
        })
      },
      onError: (e: any) => {
        toast({
          title: "Hiba",
          description: e.response?.data?.message || "Nem sikerült elküldeni az új kódot. Kérjük, próbálja újra.",
          variant: "destructive"
        })
      }
    })
  }

  const handleVerifyOTP = () => {
    if (!otp || otp.length < 4) {
      toast({
        title: "Hiba",
        description: "Kérjük, adjon meg egy érvényes OTP kódot",
        variant: "destructive"
      })
      return
    }

    // TODO: Implement OTP verification API call
    console.log('Verifying OTP:', otp, 'for email:', email)
    
    toast({
      title: "Ellenőrzés",
      description: "Az OTP kód ellenőrzése folyamatban...",
    })
  }

  return (
    <main className="min-h-screen grid place-items-center px-4">
      <Card className="w-full max-w-sm">
        <CardHeader className="text-center">
          <CardTitle className="text-2xl">OTP Ellenőrzés</CardTitle>
          <p className="text-sm text-muted-foreground">
            Kérjük, adja meg az elküldött OTP kódot
          </p>
        </CardHeader>
        <CardContent className="flex flex-col gap-6">
          {email && (
            <div className="flex items-center gap-2 justify-center p-3 bg-muted rounded-md">
              <Mail className="h-4 w-4 text-muted-foreground" />
              <div className="text-center">
                <p className="text-xs text-muted-foreground">Email cím</p>
                <p className="text-sm font-medium">{email}</p>
              </div>
            </div>
          )}

          <div className="space-y-2">
            <label htmlFor="otp" className="text-sm font-medium">
              OTP kód
            </label>
            <Input
              id="otp"
              type="text"
              placeholder="000000"
              value={otp}
              onChange={(e) => {
                const value = e.target.value.replace(/\D/g, '').slice(0, 6)
                setOtp(value)
              }}
              onKeyDown={(e) => {
                if (e.key === 'Enter') {
                  handleVerifyOTP()
                }
              }}
              maxLength={6}
              className="text-center text-2xl tracking-[0.5em] font-mono h-14"
            />
          </div>

          <Button
            onClick={handleVerifyOTP}
            disabled={!otp || otp.length < 4}
            className="w-full"
            size="lg"
          >
            Ellenőrzés
          </Button>

          <div className="text-center space-y-2">
            <button
              onClick={handleResendOTP}
              disabled={isSendingOTP || !email}
              className="text-sm text-primary hover:underline disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              {isSendingOTP ? (
                <span className="inline-flex items-center gap-2">
                  <Loader2 className="h-3 w-3 animate-spin" />
                  Új kód küldése...
                </span>
              ) : (
                'Új kód küldése'
              )}
            </button>
          </div>

          <Button
            variant="outline"
            onClick={() => navigate('/')}
            className="w-full"
          >
            <ArrowLeft className="h-4 w-4 mr-2" />
            Vissza a bejelentkezéshez
          </Button>
        </CardContent>
      </Card>
    </main>
  )
}


