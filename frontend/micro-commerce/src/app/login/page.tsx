"use client";
import { useState, FormEvent } from "react";

const Login = () => {
  const [formData, setFormData] = useState({ email: "", password: "" });
  const [errors, setErrors] = useState<{ [key: string]: string }>({});

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData({ ...formData, [name]: value });
  };

  const handleSubmit = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    // Basic form validation
    const newErrors: { [key: string]: string } = {};
    if (!formData.email.trim()) {
      newErrors.email = "Email is required";
    }
    if (!formData.password.trim()) {
      newErrors.password = "Password is required";
    }
    setErrors(newErrors);

    // Submit form if there are no errors
    if (Object.keys(newErrors).length === 0) {
      // Add your authentication logic here
      console.log("Form submitted:", formData);
    }
  };

  return (
    <div>
      <h1>Login</h1>
      <form onSubmit={handleSubmit}>
        <div>
          <label>Emafffil:</label>
          <input
            type="email"
            name="email"
            value={formData.email}
            onChange={handleChange}
          />
          {errors.email && <div style={{ color: "red" }}>{errors.email}</div>}
        </div>
        <div>
          <label>Password:</label>
          <input
            type="password"
            name="password"
            value={formData.password}
            onChange={handleChange}
          />
          {errors.password && (
            <div style={{ color: "red" }}>{errors.password}</div>
          )}
        </div>
        <button type="submit">Login</button>
      </form>
    </div>
  );
};

export default Login;
