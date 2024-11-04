"use client";
import { useState } from "react";
import {
  Container,
  Typography,
  TextField,
  Button,
  Grid,
  Link as MuiLink,
} from "@mui/material";
import { LoadingButton } from "@mui/lab";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { signIn, useSession } from "next-auth/react";

const SignInPage: React.FC = () => {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [errors, setErrors] = useState<{ [key: string]: string }>({});
  const [loading, setLoading] = useState(false);

  const router = useRouter();

  const { data: session, status } = useSession();

  const validateForm = () => {
    const newErrors: { [key: string]: string } = {};

    if (!email.trim()) {
      newErrors.email = "Email is required";
    }

    if (!password.trim()) {
      newErrors.password = "Password is required";
    }

    setErrors(newErrors);

    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const isValid = validateForm();

    if (isValid) {
      try {
        setLoading(true);
        const result = await signIn("credentials", {
          redirect: false,
          username: email,
          password,
        });

        if (result?.ok) {
          router.push("/");
        }
      } finally {
        setLoading(false);
      }
    }
  };

  return (
    <Container component="main" maxWidth="sm">
      <div className="mt-8">
        <Typography variant="h4" align="center" gutterBottom>
          Login {status}
        </Typography>
        <form onSubmit={handleSubmit}>
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
              <LoadingButton
                variant="contained"
                color="primary"
                fullWidth
                type="submit"
                loading={loading}
              >
                Login
              </LoadingButton>
            </Grid>
            <Grid item xs={12}>
              <Typography variant="body2" align="center">
                Dont have an account?{" "}
                <MuiLink component={Link} href="/register">
                  Register
                </MuiLink>
              </Typography>
            </Grid>
          </Grid>
        </form>
      </div>
    </Container>
  );
};

export default SignInPage;
