import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { AuthProvider } from 'react-oidc-context'
import { WebStorageStateStore } from 'oidc-client-ts'
import { App } from './App'

// Authorization code with PKCE against the public micro-commerce-web client; no secret in the browser.
// The AppHost passes the authority and client ID as VITE_ environment variables.
const oidcConfig = {
  authority: import.meta.env.VITE_OIDC_AUTHORITY,
  client_id: import.meta.env.VITE_OIDC_CLIENT_ID,
  redirect_uri: window.location.origin,
  post_logout_redirect_uri: window.location.origin,
  scope: 'openid profile email',
  // Session storage, never local storage: the session ends with the tab.
  userStore: new WebStorageStateStore({ store: window.sessionStorage }),
  // Drop ?code=…&state=… from the address bar once the sign-in callback is handled.
  onSigninCallback: () => {
    window.history.replaceState({}, document.title, window.location.pathname)
  },
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <AuthProvider {...oidcConfig}>
      <App />
    </AuthProvider>
  </StrictMode>,
)
