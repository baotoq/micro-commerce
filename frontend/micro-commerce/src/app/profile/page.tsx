import { getServerSession } from "next-auth";
import React from "react";

const Profile = async () => {
  const session = await getServerSession();

  return <div>{session?.user?.email}</div>;
};

export default Profile;
