const Staff = require('./models/staff.model');

const getAllStaff = async () => {
  return await Staff.find().populate('userAccount', 'username role');
};

const getStaffById = async (id) => {
  return await Staff.findById(id).populate('userAccount', 'username role');
};

const createStaff = async (staffData) => {
  const staff = new Staff(staffData);
  return await staff.save();
};

const updateStaff = async (id, staffData) => {
  return await Staff.findByIdAndUpdate(id, staffData, { new: true });
};

const deleteStaff = async (id) => {
  return await Staff.findByIdAndDelete(id);
};

module.exports = {
  getAllStaff,
  getStaffById,
  createStaff,
  updateStaff,
  deleteStaff,
};