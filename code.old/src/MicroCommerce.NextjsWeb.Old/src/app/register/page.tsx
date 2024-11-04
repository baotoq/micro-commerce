"use client";
import { useState } from "react";
import { Container, Typography, TextField, Button, Grid, Link as MuiLink } from "@mui/material";
import Link from "next/link";

import { useStripe, useElements, PaymentElement, LinkAuthenticationElement } from "@stripe/react-stripe-js";

const RegisterPage: React.FC = () => {
  const stripe = useStripe();
  const elements = useElements();
  const [message, setMessage] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [errors, setErrors] = useState<{ [key: string]: string }>({});

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!stripe || !elements) {
      // Stripe.js has not yet loaded.
      // Make sure to disable form submission until Stripe.js has loaded.
      return;
    }

    setIsLoading(true);

    const { error } = await stripe.confirmPayment({
      elements,
      confirmParams: {
        // Make sure to change this to your payment completion page
        return_url: `${window.location.origin}/completion`,
      },
    });

    // This point will only be reached if there is an immediate error when
    // confirming the payment. Otherwise, your customer will be redirected to
    // your `return_url`. For some payment methods like iDEAL, your customer will
    // be redirected to an intermediate site first to authorize the payment, then
    // redirected to the `return_url`.
    if (error.type === "card_error" || error.type === "validation_error") {
      setMessage(error.message ?? "");
    } else {
      setMessage("An unexpected error occured.");
    }

    setIsLoading(false);
  };

  const validateForm = () => {
    const newErrors: { [key: string]: string } = {};

    if (!email.trim()) {
      newErrors.email = "Email is required";
    }

    if (!password.trim()) {
      newErrors.password = "Password is required";
    }

    if (!confirmPassword.trim()) {
      newErrors.confirmPassword = "Confirm Password is required";
    }

    if (password.trim() !== confirmPassword.trim()) {
      newErrors.confirmPassword = "Passwords do not match";
    }

    setErrors(newErrors);

    return Object.keys(newErrors).length === 0;
  };

  const handleRegister = () => {
    const isValid = validateForm();

    if (isValid) {
      // Proceed with registration logic
      console.log("Registering...");
    }
  };

  return (
    <Container component="main" maxWidth="sm">
      <div className="mt-8">
        <form id="payment-form" onSubmit={handleSubmit}>
          <LinkAuthenticationElement
            id="link-authentication-element"
            // Access the email value like so:
            // onChange={(event) => {
            //  setEmail(event.value.email);
            // }}
            //
            // Prefill the email field like so:
            // options={{defaultValues: {email: 'foo@bar.com'}}}
          />
          <PaymentElement id="payment-element" />
          <button disabled={isLoading || !stripe || !elements} id="submit">
            <span id="button-text">{isLoading ? <div className="spinner" id="spinner"></div> : "Pay now"}</span>
          </button>
          {/* Show any error or success messages */}
          {message && <div id="payment-message">{message}</div>}
        </form>

        <Typography variant="h4" align="center" gutterBottom>
          Register
        </Typography>
        <form>
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <TextField
                label="Email"
                variant="outlined"
                fullWidth
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                error={!!errors.email}
                helperText={errors.email}
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                label="Password"
                type="password"
                variant="outlined"
                fullWidth
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                error={!!errors.password}
                helperText={errors.password}
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                label="Confirm Password"
                type="password"
                variant="outlined"
                fullWidth
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                error={!!errors.confirmPassword}
                helperText={errors.confirmPassword}
              />
            </Grid>
            <Grid item xs={12}>
              <Button variant="contained" color="primary" fullWidth onClick={handleRegister}>
                Register
              </Button>
            </Grid>
          </Grid>
        </form>
      </div>
    </Container>
  );
};

export default RegisterPage;
