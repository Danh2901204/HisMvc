const User = require('../auth/models/user.model');
const Staff = require('../staff/models/staff.model');
const Patient = require('../reception/models/patient.model');
const Visit = require('../reception/models/visit.model');
const LabOrder = require('../lab/models/laborder.model');

const getUsers = async () => {
  try {
    const users = await User.find().populate('staff');
    return users;
  } catch (error) {
    throw error;
  }
};

const createUser = async (userData) => {
  const { username, password, role, firstName, lastName, email, specialty } = userData;

  // 1. Create the user
  const user = new User({ username, password, role });
  await user.save();

  // 2. Create the staff member
  const staff = new Staff({
    firstName,
    lastName,
    fullName: `${firstName} ${lastName}`,
    staffCode: `ST-${Date.now()}`, // Generate a unique staff code
    email,
    specialty,
    userAccount: user._id,
  });
  await staff.save();

  // 3. Link staff to user
  user.staff = staff._id;
  await user.save();

  return user;
};

const deleteUserById = async (userId) => {
  const user = await User.findById(userId);
  if (!user) {
    throw new Error('User not found');
  }

  if (user.staff) {
    await Staff.findByIdAndDelete(user.staff);
  }

  await User.findByIdAndDelete(userId);
};

const getUserById = async (userId) => {
  return User.findById(userId).populate('staff');
};

const updateUser = async (userId, userData) => {
  const user = await User.findById(userId).populate('staff');
  if (!user) {
    throw new Error('User not found');
  }

  // Update user fields
  user.username = userData.username || user.username;
  user.role = userData.role || user.role;
  if (userData.password) {
    user.password = userData.password; // Hashing is handled by the pre-save hook in the model
  }
  await user.save();

  // Update staff fields
  if (user.staff) {
    const staff = user.staff;
    staff.firstName = userData.firstName || staff.firstName;
    staff.lastName = userData.lastName || staff.lastName;
    staff.fullName = `${staff.firstName} ${staff.lastName}`;
    staff.email = userData.email || staff.email;
    staff.specialty = userData.specialty || staff.specialty;
    await staff.save();
  }

  return user;
};

const getStatistics = async () => {
    const patientCount = await Patient.countDocuments();
    const appointmentCount = await Visit.countDocuments();
    const doctorCount = await User.countDocuments({ role: 'doctor' });
    const labOrderCount = await LabOrder.countDocuments();

    return {
        patientCount,
        appointmentCount,
        doctorCount,
        labOrderCount,
    };
};


module.exports = {
  getUsers,
  createUser,
  deleteUserById,
  getUserById,
  updateUser,
};