import { useEffect, useState } from "react";

declare global {
  interface Window {
    fbAsyncInit: () => void;
    FB: any;
  }
}

export function useFacebook(appId: string) {
  const [loaded, setLoaded] = useState(false);

  useEffect(() => {
    if (window.FB) {
      setLoaded(true);
      return;
    }

    window.fbAsyncInit = function () {
      window.FB.init({
        appId,
        cookie: true,
        xfbml: false,
        version: "v19.0",
      });
      setLoaded(true);
    };

    const script = document.createElement("script");
    script.src = "https://connect.facebook.net/en_US/sdk.js";
    script.async = true;
    script.defer = true;
    script.onload = () => console.log("FB SDK loaded");
    script.onerror = () => console.error("FB SDK load failed");
    document.body.appendChild(script);

    return () => {
      document.body.removeChild(script);
    };
  }, [appId]);

  return loaded;
}
