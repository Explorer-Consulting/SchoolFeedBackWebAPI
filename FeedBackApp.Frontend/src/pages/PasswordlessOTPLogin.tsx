import { useNavigate, useLocation } from 'react-router-dom'
import { useReviews } from '@/hooks/useReviews'
import { useAuthStore } from '@/hooks/useAuth'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { Loader2, Mail, ArrowLeft } from 'lucide-react'
import { useState, useEffect } from 'react'
import { useToast } from '@/hooks/useToast'
import { User, Role } from '@/models/User'

export default function PasswordlessOTPLogin() {
  const navigate = useNavigate()
  const location = useLocation()
  const setUser = useAuthStore((state) => state.setUser)
  const [otp, setOtp] = useState('')
  const [email, setEmail] = useState<string>('')
  const [returnTo, setReturnTo] = useState<string | null>(null) 
  const { toast } = useToast()

  const { sendOTP, isSendingOTP, verifyOTP, isVerifyingOTP } = useReviews()

  // Check if this is a self opt-in flow 
  const allowSelfOptIn = returnTo ? returnTo.includes('optin=') : false

  useEffect(() => {
    // Get email and returnTo from navigation state
    const stateEmail = location.state?.email
    const stateReturnTo = location.state?.returnTo
    
    if (stateEmail) {
      setEmail(stateEmail)
      setReturnTo(stateReturnTo || null) 
    } else {
      // If no email in state, redirect back to home
      navigate('/')
    }
  }, [location.state, navigate])

  const handleResendOTP = () => {
    if (!email) return

    // Pass allowSelfOptIn flag 
    sendOTP({ email, allowSelfOptIn }, {
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
    if (!otp || otp.length < 6 || !email) {
      toast({
        title: "Hiba",
        description: "Kérjük, adjon meg egy érvényes OTP kódot",
        variant: "destructive"
      })
      return
    }

    verifyOTP({ email, code: otp }, {
      onSuccess: (data: any) => {
        // Create user object from response
        const user: User = {
          email: data.email,
          role: data.role === 'Admin' ? Role.Admin : Role.Student,
          firstName: '', // OTP login doesn't provide name
          lastName: ''
        }
        
        // Store user in auth store
        setUser(user)
        
        toast({
          title: "Sikeres bejelentkezés",
          description: `Üdvözöljük, ${data.email}!`,
        })
        
        // Redirect based on returnTo or role 
        setTimeout(() => {
          // If returnTo exists, navigate there 
          if (returnTo) {
            navigate(returnTo)
            return
          }
          
          // Otherwise normal role-based navigation
          if (data.role === 'Admin') {
            navigate('/dashboard/admin')
          } else {
            navigate('/dashboard/student')
          }
        }, 1000)
      },
      onError: (e: any) => {
        toast({
          title: "Hiba",
          description: e.response?.data?.message || "Érvénytelen vagy lejárt OTP kód. Kérjük, próbálja újra.",
          variant: "destructive"
        })
        // Clear OTP on error
        setOtp('')
      }
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
            disabled={!otp || otp.length < 6 || isVerifyingOTP}
            className="w-full"
            size="lg"
          >
            {isVerifyingOTP ? (
              <>
                <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                Ellenőrzés...
              </>
            ) : (
              'Ellenőrzés'
            )}
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