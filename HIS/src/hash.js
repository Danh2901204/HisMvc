const bcrypt = require("bcrypt");

async function run() {
  const passwords = {
    admin: "admin123",
    reception01: "reception123",
    doctor01: "doctor123"
  };

  for (const [user, pass] of Object.entries(passwords)) {
    const hash = await bcrypt.hash(pass, 10);
    console.log(user, "=>", hash);
  }
}

run();
