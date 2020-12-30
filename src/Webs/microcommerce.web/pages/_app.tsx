import { Provider } from "react-redux";
import store from "~/store";

import "~/styles/globals.css";

function App({ Component, pageProps }) {
  return (
    <Provider store={store}>
      <Component {...pageProps} />
    </Provider>
  );
}

export default App;
