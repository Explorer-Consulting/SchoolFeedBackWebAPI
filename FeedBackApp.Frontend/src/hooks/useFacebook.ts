import { useEffect } from "react"

declare global {
  interface Window {
    fbAsyncInit: () => void
    FB: any
  }
}

export function useFacebook(appId: string) {
  useEffect(() => {
    window.fbAsyncInit = function () {
      window.FB.init({
        appId,
        cookie: true,
        xfbml: true,
        version: "v21.0",
      })
    }
  }, [appId])

  const login = (callback: (accessToken: string) => void, onError: () => void) => {
    window.FB.login(
      (response: any) => {
        if (response.status === "connected") {
          const accessToken = response.authResponse.accessToken
          callback(accessToken)
        } else {
          onError()
        }
      },
      { scope: "email,public_profile" }
    )
  }

  return { login }
}
