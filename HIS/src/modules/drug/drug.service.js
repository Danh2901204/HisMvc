const Drug = require('../pharmacy/models/drug.model');

const getAllDrugs = async () => {
  return await Drug.find();
};

const getDrugById = async (id) => {
  return await Drug.findById(id);
};

const createDrug = async (drugData) => {
  const drug = new Drug(drugData);
  return await drug.save();
};

const updateDrug = async (id, drugData) => {
  return await Drug.findByIdAndUpdate(id, drugData, { new: true });
};

const deleteDrug = async (id) => {
  return await Drug.findByIdAndDelete(id);
};

module.exports = {
  getAllDrugs,
  getDrugById,
  createDrug,
  updateDrug,
  deleteDrug,
};