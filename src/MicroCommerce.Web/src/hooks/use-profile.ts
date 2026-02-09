"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import { toast } from "sonner";
import {
  getMyProfile,
  updateProfile,
  uploadAvatar,
  removeAvatar,
  addAddress,
  updateAddress,
  deleteAddress,
  setDefaultAddress,
} from "@/lib/api";
import type { AddAddressRequest, UpdateAddressRequest, UpdateProfileRequest } from "@/lib/api";

export function useProfile() {
  const { data: session } = useSession();

  return useQuery({
    queryKey: ["profile"],
    queryFn: () => getMyProfile(session?.accessToken),
    enabled: !!session?.accessToken,
  });
}

export function useUpdateProfile() {
  const queryClient = useQueryClient();
  const { data: session } = useSession();

  return useMutation({
    mutationFn: (data: UpdateProfileRequest) => updateProfile(data, session?.accessToken),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["profile"] });
      toast.success("Profile updated");
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to update profile");
    },
  });
}

export function useUploadAvatar() {
  const queryClient = useQueryClient();
  const { data: session } = useSession();

  return useMutation({
    mutationFn: (file: File) => uploadAvatar(file, session?.accessToken),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["profile"] });
      toast.success("Avatar updated");
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to upload avatar");
    },
  });
}

export function useRemoveAvatar() {
  const queryClient = useQueryClient();
  const { data: session } = useSession();

  return useMutation({
    mutationFn: () => removeAvatar(session?.accessToken),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["profile"] });
      toast.success("Avatar removed");
    },
    onError: () => {
      toast.error("Failed to remove avatar");
    },
  });
}

export function useAddAddress() {
  const queryClient = useQueryClient();
  const { data: session } = useSession();

  return useMutation({
    mutationFn: (data: AddAddressRequest) => addAddress(data, session?.accessToken),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["profile"] });
      toast.success("Address added");
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to add address");
    },
  });
}

export function useUpdateAddress() {
  const queryClient = useQueryClient();
  const { data: session } = useSession();

  return useMutation({
    mutationFn: ({ id, ...data }: UpdateAddressRequest & { id: string }) =>
      updateAddress(id, data, session?.accessToken),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["profile"] });
      toast.success("Address updated");
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to update address");
    },
  });
}

export function useDeleteAddress() {
  const queryClient = useQueryClient();
  const { data: session } = useSession();

  return useMutation({
    mutationFn: (addressId: string) => deleteAddress(addressId, session?.accessToken),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["profile"] });
      toast.success("Address deleted");
    },
    onError: () => {
      toast.error("Failed to delete address");
    },
  });
}

export function useSetDefaultAddress() {
  const queryClient = useQueryClient();
  const { data: session } = useSession();

  return useMutation({
    mutationFn: (addressId: string) => setDefaultAddress(addressId, session?.accessToken),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["profile"] });
      toast.success("Default address updated");
    },
    onError: () => {
      toast.error("Failed to set default address");
    },
  });
}
